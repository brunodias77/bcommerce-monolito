namespace Bcommerce.BuildingBlocks.Infrastructure.BackgroundJobs;

// Interface marcadora ou com contrato específico se não usarmos IJob do Quartz direto no domínio
public interface IBackgroundJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
