namespace ChrlsChn.MoMo.Setup;

public static class RuntimeEnv {
  /// <summary>
  /// True when either `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`
  /// are present in the environment variables and equal to "Development"
  /// </summary>
  public static bool IsDevelopment =>
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
      == Environments.Development
    || Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
      == Environments.Development;
}
