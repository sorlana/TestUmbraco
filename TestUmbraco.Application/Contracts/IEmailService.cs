using TestUmbraco.Application.DTO;

namespace TestUmbraco.Application.Contracts
{
    public interface IEmailService
    {
        Task<bool> SendFormSubmissionAsync(
            FormSubmissionDto submission,
            string recipientEmail,
            string subject);
    }
}
