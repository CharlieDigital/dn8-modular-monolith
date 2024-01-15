using System.Diagnostics.CodeAnalysis;

namespace ChrlsChn.MoMo.Setup;

/// <summary>
/// Configuration model for the database
/// </summary>
public class Database {
  [NotNull]
  public string? ConnectionString { get; init; }

  /// <summary>
  /// When executing in the container, we need to use the container name.
  /// </summary>
  public string ResolveConnectionString() {
    if (RuntimeEnv.IsDevelopment) {
      return ConnectionString;
    }

    // When running in a container, we need to use the name of the service.
    // Upstream in Cloud Run, for example, we would override the configuration.
    return ConnectionString.Replace("127.0.0.1", "postgres");
  }
};

/// <summary>
/// Configuration model for the application.
/// </summary>
public class MoMoConfig {
  [NotNull]
  public Database? Database { get; init; }
};
