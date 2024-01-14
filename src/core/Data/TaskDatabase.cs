using ChrlsChn.MoMo.Data.Model;
using ChrlsChn.MoMo.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChrlsChn.MoMo.Data;

/// <summary>
/// Partial class for the database which contains the main setup code.
/// </summary>
public partial class TaskDatabase : DbContext {
  private readonly string? _connectionString;

  public TaskDatabase(
    IOptions<MoMoConfig> options
  ) {
    _connectionString = options.Value.Database.ResolveConnectionString();
  }

  public TaskDatabase(string connectionString) {
    _connectionString = connectionString;
  }

  public TaskDatabase(
    string connectionString,
    DbContextOptions<TaskDatabase> options
  ) : base(options) {
    _connectionString = connectionString;
  }

  protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder
  ) {
    if (optionsBuilder.IsConfigured) {
      return;
    }

    optionsBuilder
      .UseNpgsql(_connectionString,
        o => o.UseAdminDatabase("postgres"))
      .UseSnakeCaseNamingConvention()
      .EnableDetailedErrors()
      .EnableSensitiveDataLogging();
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<WorkItem>()
      .HasMany(e => e.Users)
      .WithMany(e => e.WorkItems)
      .UsingEntity<WorkItemUser>();
  }
}
