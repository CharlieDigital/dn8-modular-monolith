using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Setup;
using Microsoft.Extensions.Options;

namespace ChrlsChn.Momo.Services;

/// <summary>
/// Base class for our services.
/// </summary>
public abstract class BackgroundServiceBase(IOptions<MoMoConfig> config) : BackgroundService {
  protected TaskDatabase Database { get; } = new(config.Value.Database.ResolveConnectionString());

  /// <summary>
  /// Starts execution of the background service.
  /// </summary>
  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    using var timer = new PeriodicTimer(
      TimeSpan.FromMilliseconds(Interval)
    );

    try {
      Console.WriteLine($"Starting service: {GetType().Name}");

      // Executes the `RunAsync` method on each tick of the timer.
      while (await timer.WaitForNextTickAsync(stoppingToken)) {
        await RunAsync(stoppingToken);
      }
    } catch (OperationCanceledException) {
      Console.WriteLine($"Stopping service: {GetType().Name}");
    } finally {
      await Database.DisposeAsync();
    }

    Console.WriteLine($"Stopped service: {GetType().Name}");
  }

  /// <summary>
  /// The timer interval for the services to execute.
  /// </summary>
  protected abstract int Interval { get; }

  /// <summary>
  /// Inheriting classes implement the logic of the service in this method.
  /// </summary>
  protected abstract Task RunAsync(CancellationToken stoppingToken);
}
