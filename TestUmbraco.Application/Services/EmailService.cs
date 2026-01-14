using TestUmbraco.Application.Contracts;
using TestUmbraco.Application.DTO;
using TestUmbraco.Domain.Contracts;
using TestUmbraco.Domain.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace TestUmbraco.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IRepository<CallRequestItem> _callRequestRepository;
        private readonly IRepository<EmailRequestItem> _emailRequestRepository;
        private readonly string _emailSenderAddress;
        private readonly string _emailSenderName;
        private readonly string _emailSenderPassword;
        private readonly string _emailReceiverAddress;
        private readonly string _emailReceiverName;
        private readonly string _emailSubject;
        private readonly string _smtpServer;
        private readonly int _smtpPort;

        public EmailService(
            IRepository<CallRequestItem> callRequestRepository,
            IRepository<EmailRequestItem> emailRequestRepository,
            ILogger<EmailService> logger,
            IConfiguration config)
        {
            _callRequestRepository = callRequestRepository ?? throw new ArgumentNullException(nameof(callRequestRepository));
            _emailRequestRepository = emailRequestRepository ?? throw new ArgumentNullException(nameof(emailRequestRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Безопасное получение значений из конфигурации
            _emailSenderAddress = config["Email:EmailSenderAddress"] ?? string.Empty;
            _emailSenderName = config["Email:EmailSenderName"] ?? string.Empty;
            _emailSenderPassword = config["Email:EmailSenderPassword"] ?? string.Empty;
            _emailReceiverAddress = config["Email:EmailReceiverAddress"] ?? string.Empty;
            _emailReceiverName = config["Email:EmailReceiverName"] ?? string.Empty;
            _emailSubject = config["Email:EmailSubject"] ?? "Новое обращение";
            _smtpServer = config["Email:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = config.GetValue<int?>("Email:SmtpPort") ?? 465; // Порт по умолчанию для SSL

            // Проверка критически важных параметров
            if (string.IsNullOrWhiteSpace(_emailSenderAddress) || 
                string.IsNullOrWhiteSpace(_emailSenderPassword) ||
                string.IsNullOrWhiteSpace(_emailReceiverAddress))
            {
                _logger.LogWarning("Не все параметры email настроены в конфигурации");
            }
        }

        public async Task SendEmailRequestAsync(EmailRequestDto emailRequest)
        {
            if (emailRequest == null)
                throw new ArgumentNullException(nameof(emailRequest));

            var mailItem = new EmailRequestItem
            {
                Name = emailRequest.Name ?? string.Empty,
                Phone = emailRequest.Phone ?? string.Empty,
                Email = emailRequest.Email ?? string.Empty,
                Comment = emailRequest.Comment ?? string.Empty
            };

            try
            {
                await _emailRequestRepository.AddAsync(mailItem);
                
                var message = new MimeMessage();
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"<b>Имя:</b> {mailItem.Name}<br><br>" +
                              $"<b>Тел:</b> {mailItem.Phone}<br><br>" +
                              $"<b>Почта:</b> {mailItem.Email}<br><br>" +
                              $"<b>Комментарий:</b> {mailItem.Comment}"
                };
                message.Body = bodyBuilder.ToMessageBody();
                
                await SendEmailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка при отправке email запроса: {Message}", e.Message);
                throw;
            }
        }

        public async Task SendCallRequestAsync(CallRequestDto callRequest)
        {
            if (callRequest == null)
                throw new ArgumentNullException(nameof(callRequest));

            var mailItem = new CallRequestItem
            {
                Phone = callRequest.Phone ?? string.Empty,
                TimeCall = callRequest.TimeCall // DateTime не может быть null
            };

            try
            {
                await _callRequestRepository.AddAsync(mailItem);
                
                var message = new MimeMessage();
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $"<b>Тел:</b> {mailItem.Phone}<br><br>" +
                              $"<b>Удобно позвонить:</b> {mailItem.TimeCall:HH:mm dd.MM.yyyy}"
                };
                message.Body = bodyBuilder.ToMessageBody();
                
                await SendEmailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка при отправке запроса на звонок: {Message}", e.Message);
                throw;
            }
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Проверка, что email параметры настроены
            if (string.IsNullOrWhiteSpace(_emailSenderAddress) || 
                string.IsNullOrWhiteSpace(_emailReceiverAddress))
            {
                _logger.LogError("Невозможно отправить email: не настроены параметры отправителя или получателя");
                return;
            }

            using var client = new MailKit.Net.Smtp.SmtpClient(); // Явное указание пространства имен

            try
            {
                // Установка отправителя и получателя
                message.From.Clear();
                message.From.Add(new MailboxAddress(_emailSenderName, _emailSenderAddress));
                
                message.To.Clear();
                message.To.Add(new MailboxAddress(_emailReceiverName, _emailReceiverAddress));
                
                message.Subject = _emailSubject;

                await client.ConnectAsync(_smtpServer, _smtpPort, true);
                
                if (!string.IsNullOrWhiteSpace(_emailSenderPassword))
                {
                    await client.AuthenticateAsync(_emailSenderAddress, _emailSenderPassword);
                }

                await client.SendAsync(message);
                _logger.LogInformation("Email успешно отправлен: {Subject}", message.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке email: {Message}", ex.Message);
                throw;
            }
            finally
            {
                // Гарантированное отключение клиента
                if (client.IsConnected)
                {
                    try
                    {
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Ошибка при отключении SMTP клиента");
                    }
                }
            }
        }
    }
}