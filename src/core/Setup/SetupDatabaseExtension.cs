using ChrlsChn.MoMo.Data;

namespace ChrlsChn.MoMo.Setup;

/// <summary>
/// Extension methods for setting up the database.
/// </summary>
public static class SetupDatabaseExtension {
  public static void AddDataStore(
    this IServiceCollection services
  ) {
    services.AddScoped<TaskDatabase>();
  }
}
