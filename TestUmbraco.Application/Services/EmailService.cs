using TestUmbraco.Application.Contracts;
using TestUmbraco.Application.DTO;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text;

namespace TestUmbraco.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _emailSenderAddress;
        private readonly string _emailSenderName;
        private readonly string _emailSenderPassword;
        private readonly string _smtpServer;
        private readonly int _smtpPort;

        public EmailService(
            ILogger<EmailService> logger,
            IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Безопасное получение значений из конфигурации
            _emailSenderAddress = config["Email:EmailSenderAddress"] ?? string.Empty;
            _emailSenderName = config["Email:EmailSenderName"] ?? string.Empty;
            _emailSenderPassword = config["Email:EmailSenderPassword"] ?? string.Empty;
            _smtpServer = config["Email:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = config.GetValue<int?>("Email:SmtpPort") ?? 465; // Порт по умолчанию для SSL

            // Проверка критически важных параметров
            if (string.IsNullOrWhiteSpace(_emailSenderAddress) || 
                string.IsNullOrWhiteSpace(_emailSenderPassword))
            {
                _logger.LogWarning("Не все параметры email настроены в конфигурации");
            }
        }

        public async Task<bool> SendFormSubmissionAsync(
            FormSubmissionDto submission,
            string recipientEmail,
            string subject)
        {
            _logger.LogInformation("=== SendFormSubmissionAsync CALLED ===");
            _logger.LogInformation("Recipient: {RecipientEmail}", recipientEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("FormTitle: {FormTitle}", submission?.FormTitle);
            
            if (submission == null)
                throw new ArgumentNullException(nameof(submission));

            if (string.IsNullOrWhiteSpace(recipientEmail))
                throw new ArgumentException("Recipient email cannot be empty", nameof(recipientEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be empty", nameof(subject));

            try
            {
                _logger.LogInformation("Building HTML body...");
                var htmlBody = BuildFormSubmissionHtml(submission);

                _logger.LogInformation("Creating MimeMessage...");
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSenderName, _emailSenderAddress));
                message.To.Add(MailboxAddress.Parse(recipientEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                _logger.LogInformation("Connecting to SMTP server {SmtpServer}:{SmtpPort}...", _smtpServer, _smtpPort);
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpServer, _smtpPort, true);

                _logger.LogInformation("Authenticating with {EmailSenderAddress}...", _emailSenderAddress);
                if (!string.IsNullOrWhiteSpace(_emailSenderPassword))
                {
                    await client.AuthenticateAsync(_emailSenderAddress, _emailSenderPassword);
                }

                _logger.LogInformation("Sending email...");
                await client.SendAsync(message);
                _logger.LogInformation("Form submission email successfully sent for form {FormTitle} to {RecipientEmail}", 
                    submission.FormTitle, recipientEmail);

                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending form submission email for form {FormTitle}: {Message}", 
                    submission.FormTitle, ex.Message);
                return false;
            }
        }

        private string BuildFormSubmissionHtml(FormSubmissionDto submission)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; }");
            sb.AppendLine("h2 { color: #333; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            sb.AppendLine("td { border: 1px solid #ddd; padding: 8px; }");
            sb.AppendLine("td:first-child { background-color: #f2f2f2; font-weight: bold; width: 30%; }");
            sb.AppendLine(".logo { margin-bottom: 20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            
            // Логотип (если указан)
            if (!string.IsNullOrEmpty(submission.LogoUrl))
            {
                sb.AppendLine("<div class=\"logo\">");
                sb.AppendLine($"<img src=\"{submission.LogoUrl}\" alt=\"\" style=\"max-width: 200px; max-height: 100px; height: auto; display: block;\" />");
                sb.AppendLine("</div>");
            }
            
            // Заголовок с названием формы
            sb.AppendLine($"<h2>{submission.FormTitle}</h2>");
            
            // Дата и время отправки
            sb.AppendLine($"<p><strong>Дата отправки:</strong> {submission.SubmittedAt:dd.MM.yyyy HH:mm}</p>");
            
            sb.AppendLine("<hr/>");
            sb.AppendLine("<h3>Данные формы:</h3>");
            
            // Таблица с полями формы
            sb.AppendLine("<table>");
            foreach (var field in submission.FieldValues)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{field.Key}</td>");
                sb.AppendLine($"<td>{field.Value}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}