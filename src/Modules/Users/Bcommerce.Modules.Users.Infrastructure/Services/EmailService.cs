namespace Bcommerce.Modules.Users.Infrastructure.Services;

public class EmailService
{
    // Interface to be defined later or injected via building blocks
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Stub implementation
        Console.WriteLine($"[EmailStub] To: {to}, Subject: {subject}");
        return Task.CompletedTask;
    }
}
