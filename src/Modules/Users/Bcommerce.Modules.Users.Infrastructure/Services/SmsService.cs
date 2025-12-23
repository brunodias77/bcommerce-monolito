namespace Bcommerce.Modules.Users.Infrastructure.Services;

public class SmsService
{
    // Interface to be defined later or injected via building blocks
    public Task SendSmsAsync(string to, string message)
    {
        // Stub implementation
        Console.WriteLine($"[SmsStub] To: {to}, Message: {message}");
        return Task.CompletedTask;
    }
}
