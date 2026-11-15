using Provisioning.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(o => o.ServiceName = "ProvisioningWorker");
builder.Services.AddHostedService<Worker>();
builder.Build().Run();
