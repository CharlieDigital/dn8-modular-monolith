using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Setup;
using Microsoft.Extensions.Options;

namespace ChrlsChn.Momo.Services;

public abstract class BackgroundServiceBase(IOptions<MoMoConfig> config) : BackgroundService {
  protected TaskDatabase Database { get; } = new(config.Value.Database.ResolveConnectionString());

  protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
    using var timer = new PeriodicTimer(
      TimeSpan.FromMilliseconds(Interval)
    );

    try {
      Console.WriteLine($"Starting service: {GetType().Name}");

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

  protected abstract int Interval { get; }

  protected abstract Task RunAsync(CancellationToken stoppingToken);
}
