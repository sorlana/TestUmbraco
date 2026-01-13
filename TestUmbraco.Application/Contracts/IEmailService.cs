using TestUmbraco.Application.DTO;

namespace TestUmbraco.Application.Contracts
{
    public interface IEmailService
    {
        Task SendEmailRequestAsync(EmailRequestDto emailRequest);
        Task SendCallRequestAsync(CallRequestDto callRequest);
    }
}
