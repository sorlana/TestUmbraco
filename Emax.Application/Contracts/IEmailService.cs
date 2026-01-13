using Emax.Application.DTO;

namespace Emax.Application.Contracts
{
    public interface IEmailService
    {
        Task SendEmailRequestAsync(EmailRequestDto emailRequest);
        Task SendCallRequestAsync(CallRequestDto callRequest);
    }
}
