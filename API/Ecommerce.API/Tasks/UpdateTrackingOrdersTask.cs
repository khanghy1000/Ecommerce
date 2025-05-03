using Ecommerce.Application.SalesOrders.Commands;
using MediatR;

namespace Ecommerce.API.Tasks;

public class UpdateTrackingOrdersTask(
    IServiceProvider serviceProvider,
    ILogger<UpdateTrackingOrdersTask> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        do
        {
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            if (mediator == null)
            {
                logger.LogError("Mediator not found");
                continue;
            }

            await mediator.Send(new UpdateTrackingOrdersStatus.Command(), stoppingToken);
            logger.LogInformation("Updated tracking orders status");
        } while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        );
    }
}
