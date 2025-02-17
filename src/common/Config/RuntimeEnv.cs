using Microsoft.Extensions.Hosting;

namespace ChrlsChn.MoMo.Common.Config;

public static class RuntimeEnv
{
    /// <summary>
    /// True when either `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`
    /// are present in the environment variables and equal to "Development"
    /// </summary>
    public static bool IsDevelopment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development
        || Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Environments.Development;

    /// <summary>
    /// True when running code generation.
    /// </summary>
    public static bool IsCodegen =>
        Environment.GetEnvironmentVariable("GEN")?.Trim().ToLowerInvariant() == "true";

    /// <summary>
    /// True when the DB does not need to be reset.  If we are running multiple DB nodes, we don't need to keep resetting it.
    /// </summary>
    public static bool SkipDbReset =>
        Environment.GetEnvironmentVariable("SKIP_DB_RESET")?.Trim().ToLowerInvariant() == "true";

    /// <summary>
    /// Just for dev purposes, we want to expose this so we can see it locally.
    /// /// </summary>
    public static bool ExposeSwaggerUI =>
        Environment.GetEnvironmentVariable("EXPOSE_SWAGGER_UI")?.Trim().ToLowerInvariant()
        == "true";
}
