using ChrlsChn.MoMo.Common.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChrlsChn.Momo.Services;

/// <summary>
/// When a task gets created, we want to notify the collaborators.  Instead of a micro-service
/// we just use it as a module in the monolith.
/// </summary>
public class WorkItemMonitorService(IOptions<MoMoConfig> options) : BackgroundServiceBase(options)
{
    protected override int Interval => 2500;

    /// <summary>
    /// A simple in-memory store for the items we've already processed;
    /// just a mock up for this demo.  Could be stored elsewhere in the DB.
    /// </summary>
    private readonly List<Guid> _processed = [];

    /// <summary>
    /// We'll monitor and find all work items that we haven't processed and simulate notifying the
    /// users for that work item.
    /// </summary>
    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMilliseconds(Interval));

        var workItems = await Database
            .WorkItems.Include(w => w.Users)
            .Include(w => w.Project)
            .Where(w => w.CreatedUtc > cutoff)
            .ToListAsync(cancellationToken: stoppingToken);

        Parallel.ForEach(
            workItems,
            (w) =>
            {
                if (_processed.Contains(w.Id))
                {
                    return; // We already processed this.
                }

                // Simulate notifying the users.
                Console.WriteLine($"[NEW] Found new task {w.Name} on project {w.Project.Name}.");

                foreach (var collaborator in w.Users)
                {
                    Console.WriteLine($"  ⮑  Notifying user {collaborator.Name}");
                }

                _processed.Add(w.Id);
            }
        );
    }
}
