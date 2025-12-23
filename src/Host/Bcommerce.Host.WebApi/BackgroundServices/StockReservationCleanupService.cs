using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bcommerce.Host.WebApi.BackgroundServices;

public class StockReservationCleanupService : BackgroundService
{
    private readonly ILogger<StockReservationCleanupService> _logger;

    public StockReservationCleanupService(ILogger<StockReservationCleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Reservation Cleanup Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Running stock reservation cleanup...");
            // Logic to cleanup expired reservations would go here
            // var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
            // await stockService.CleanupExpiredReservations();
            
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
