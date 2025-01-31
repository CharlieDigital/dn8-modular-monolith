// This runtime does not load the REST API endpoints and only loads the services

using ChrlsChn.Momo.Services;
using ChrlsChn.MoMo.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MoMoConfig>(builder.Configuration.GetSection(nameof(MoMoConfig)));

var role = Environment.GetEnvironmentVariable("SVC_ROLE");

if (role == nameof(WorkItemMonitorService))
{
    // Run single service
    Console.WriteLine("Loading WorkItemMonitorService service");
    builder.Services.AddHostedService<WorkItemMonitorService>();
}
else if (role == nameof(WorkItemStatusMonitorService))
{
    // Run single service
    Console.WriteLine("Loading WorkItemStatusMonitorService service");
    builder.Services.AddHostedService<WorkItemStatusMonitorService>();
}
else
{
    // Run both services
    Console.WriteLine("Loading both services");
    builder.Services.AddHostedService<WorkItemMonitorService>();
    builder.Services.AddHostedService<WorkItemStatusMonitorService>();
}

builder.Services.AddDataStore();

var host = builder.Build();
host.Run();
