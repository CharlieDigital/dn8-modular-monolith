using ChrlsChn.MoMo.Data.Model;
using ChrlsChn.MoMo.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChrlsChn.MoMo.Data;

/// <summary>
/// Partial class for the database which contains the main setup code.
/// </summary>
public partial class TaskDatabase : DbContext
{
    private readonly string? _connectionString;

    public TaskDatabase(IOptions<MoMoConfig> options)
    {
        _connectionString = options.Value.Database.ResolveConnectionString();
    }

    public TaskDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public TaskDatabase(string connectionString, DbContextOptions<TaskDatabase> options)
        : base(options)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Set up the options for the database
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        if (RuntimeEnv.IsCodegen)
        {
            Console.WriteLine("  ⮑  Starting database in codegen");

            // 👇 For codegen, we simply connect an in-memory database for DI
            optionsBuilder.UseInMemoryDatabase("codegen");
        }
        else
        {
            optionsBuilder
                .UseNpgsql(_connectionString, o => o.UseAdminDatabase("postgres"))
                .UseSnakeCaseNamingConvention()
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }
    }

    /// <summary>
    /// Configure the many-to-many mapping with access to the mapping table
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<WorkItem>()
            .HasMany(e => e.Users)
            .WithMany(e => e.WorkItems)
            .UsingEntity<WorkItemUser>();
    }
}
