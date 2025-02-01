using ChrlsChn.MoMo.Common.Config;
using ChrlsChn.Momo.Services;

namespace ChrlsChn.MoMo.Setup;

public static class SetupServicesExtension
{
    /// <summary>
    /// Performs the service setup.
    /// </summary>
    public static void AddCustomServices(this IServiceCollection services)
    {
        // 👇 In the development environment, we load BOTH services into the runtime
        if (RuntimeEnv.IsDevelopment && !RuntimeEnv.IsCodegen)
        {
            services.AddHostedService<WorkItemMonitorService>();

            services.AddHostedService<WorkItemStatusMonitorService>();
        }
    }
}
