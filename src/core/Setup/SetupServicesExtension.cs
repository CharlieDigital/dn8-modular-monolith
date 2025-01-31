using ChrlsChn.Momo.Services;

namespace ChrlsChn.MoMo.Setup;

public static class SetupServicesExtension
{
    /// <summary>
    /// Performs the service setup.
    /// </summary>
    public static void AddCustomServices(this IServiceCollection services)
    {
        if (RuntimeEnv.IsDevelopment && !RuntimeEnv.IsCodegen)
        {
            services.AddHostedService<WorkItemMonitorService>();

            services.AddHostedService<WorkItemStatusMonitorService>();
        }
    }
}
