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

        public EmailService(IRepository<CallRequestItem> callRequestRepository,
            IRepository<EmailRequestItem> emailRequestRepository, ILogger<EmailService> logger,
            IConfiguration config)
        {
            _callRequestRepository = callRequestRepository;
            _emailRequestRepository = emailRequestRepository;
            _logger = logger;
            _emailSenderAddress = config.GetRequiredSection("Email:EmailSenderAddress").Value;
            _emailSenderName = config.GetRequiredSection("Email:EmailSenderName").Value;
            _emailSenderPassword = config.GetRequiredSection("Email:EmailSenderPassword").Value;
            _emailReceiverAddress = config.GetRequiredSection("Email:EmailReceiverAddress").Value;
            _emailReceiverName = config.GetRequiredSection("Email:EmailReceiverName").Value;
            _emailSubject = config.GetRequiredSection("Email:EmailSubject").Value;
            _smtpServer = config.GetRequiredSection("Email:SmtpServer").Value;
        }

        public async Task SendEmailRequestAsync(EmailRequestDto emailRequest)
        {
            var mailItem = new EmailRequestItem
            {
                Name = emailRequest.Name,
                Phone = emailRequest.Phone,
                Email = emailRequest.Email,
                Comment = emailRequest.Comment
            };

            try
            {
                await _emailRequestRepository.AddAsync(mailItem);
                var message = new MimeMessage();
                message.Body = new BodyBuilder() { HtmlBody = string.Format("<b>Имя:</b> {0} <br><br> <b>Тел:</b> {1} <br><br> <b>Почта:</b> {2} <br><br> <b>Комментарий:</b> {3}", emailRequest.Name, emailRequest.Phone, emailRequest.Email, emailRequest.Comment) }.ToMessageBody(); //тело сообщения (так же в формате HTML)
                await SendEmailAsync(message);

            }
            catch (Exception e)
            {
                _logger.LogError(e.GetBaseException().Message);
            }

        }


        public async Task SendCallRequestAsync(CallRequestDto callRequest)
        {
            var mailItem = new CallRequestItem
            {
                Phone = callRequest.Phone,
                TimeCall = callRequest.TimeCall
            };
            try
            {
                await _callRequestRepository.AddAsync(mailItem);
                MimeMessage message = new MimeMessage();
                message.Body = new BodyBuilder() { HtmlBody = string.Format("<b>Тел:</b> {0} <br><br> <b>Удобно позвонить:</b> {1} ", callRequest.Phone, callRequest.TimeCall) }.ToMessageBody(); //тело сообщения (так же в формате HTML)
                await SendEmailAsync(message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.GetBaseException().Message);
            }

        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            message.From.Add(new MailboxAddress(_emailSenderName, _emailSenderAddress)); //отправитель сообщения
            message.To.Add(new MailboxAddress(_emailReceiverName, _emailReceiverAddress)); //адресат сообщения
            message.Subject = _emailSubject; //тема сообщения                

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, 465, true);
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_emailSenderAddress, _emailSenderPassword);//логин-пароль от аккаунта
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Сообщение отправлено успешно!");
            }
        }
    }
}
