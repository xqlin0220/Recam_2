using Remp.Service.DTOs;

namespace Remp.Service.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(EmailRequestDto request);
}
