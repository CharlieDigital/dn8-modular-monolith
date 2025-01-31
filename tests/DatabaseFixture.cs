using ChrlsChn.MoMo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace tests;

public class DatabaseFixture
{
    private static readonly string _connectionString =
        "server=127.0.0.1;port=6543;database=unit_test;user id=postgres;password=postgres;include error detail=true";

    private static readonly object _sync = new();
    private static bool _initialized;

    /// <summary>
    ///     Constructor which initializes the database for testing each run.
    /// </summary>
    public DatabaseFixture()
    {
        // Use simple double lock-check mechanism.
        if (_initialized)
        {
            return;
        }

        lock (_sync)
        {
            if (_initialized)
            {
                return;
            }

            using var dataStore = CreateDataStore();
            // Drop and create the database for each test run.
            Console.WriteLine("Creating test database...");
            dataStore.Database.EnsureDeleted();
            dataStore.Database.EnsureCreated();
            Console.WriteLine("Dropped and created test database.");

            _initialized = true;
        }
    }

    public static TaskDatabase CreateDataStore(bool logSql = false)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);

        if (logSql)
        {
            dataSourceBuilder.UseLoggerFactory(
                LoggerFactory.Create(builder => builder.AddConsole())
            );
        }

        var dataSource = dataSourceBuilder.Build();

        var config = new DbContextOptionsBuilder<TaskDatabase>()
            .UseNpgsql(
                dataSource,
                o =>
                {
                    o.UseAdminDatabase("postgres");
                }
            )
            .UseSnakeCaseNamingConvention()
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();

        return new TaskDatabase(_connectionString, config.Options);
    }
}
