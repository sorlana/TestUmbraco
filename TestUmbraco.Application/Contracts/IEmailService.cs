using TestUmbraco.Application.DTO;

namespace TestUmbraco.Application.Contracts
{
    public interface IEmailService
    {
        Task SendEmailRequestAsync(EmailRequestDto emailRequest);
        Task SendCallRequestAsync(CallRequestDto callRequest);
        Task<bool> SendFormSubmissionAsync(
            FormSubmissionDto submission,
            string recipientEmail,
            string subject);
    }
}
