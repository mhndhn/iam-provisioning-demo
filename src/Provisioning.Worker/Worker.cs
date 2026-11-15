using Provisioning.Core;

namespace Provisioning.Worker;

public class Worker(ILogger<Worker> logger, IHostEnvironment env) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ProvisioningWorker started in {Env}", env.EnvironmentName);
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = new ProvisioningRequest { UserId = "jdoe", TargetSystem = "ActiveDirectory" };
            request.StartProcessing();
            request.Complete();
            logger.LogInformation("Request {Id} -> {Status}", request.Id, request.Status);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
