using ChrlsChn.Momo.Services;
using ChrlsChn.MoMo.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MoMoConfig>(
  builder.Configuration.GetSection(nameof(MoMoConfig))
);

builder.Services.AddHostedService<WorkItemStatusMonitorService>();
builder.Services.AddDataStore();

var host = builder.Build();
host.Run();
