namespace Bcommerce.BuildingBlocks.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
