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
