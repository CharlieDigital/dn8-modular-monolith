using ChrlsChn.MoMo.Data.Model;
using ChrlsChn.MoMo.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChrlsChn.Momo.Services;

/// <summary>
/// When a task gets completed, we want to check if all of the tasks for
/// the project are completed and then update the project status if so
/// Instead of a micro-service, we just us it as a module in the monolith.
/// </summary>
public class WorkItemStatusMonitorService(IOptions<MoMoConfig> options)
    : BackgroundServiceBase(options)
{
    protected override int Interval => 3000;

    /// <summary>
    /// A simple in-memory store for the items we've already processed;
    /// just a mock up for this demo.  Could be stored elsewhere in the DB.
    /// </summary>
    private readonly List<Guid> _processed = [];

    /// <summary>
    /// We'll monitor and find all work items that we've updated and check
    /// to see if all of the tasks on the project are completed and if so,
    /// update the status of the project.
    /// </summary>
    protected override async Task RunAsync(CancellationToken stoppingToken)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMilliseconds(Interval));

        var workItems = await Database
            .WorkItems.Include(w => w.Users)
            .Include(w => w.Project)
            .ThenInclude(p => p.WorkItems)
            .Where(w => w.UpdatedUtc > cutoff)
            .ToListAsync(cancellationToken: stoppingToken);

        await Parallel.ForEachAsync(
            workItems,
            stoppingToken,
            async (w, c) =>
            {
                if (_processed.Contains(w.Id))
                {
                    return; // We already processed this.
                }

                // Simulate notifying the users.
                Console.WriteLine(
                    $"[STATUS] Found updated task {w.Name} on project {w.Project.Name}."
                );

                if (w.Project.WorkItems.TrueForAll(i => i.Status == WorkItemStatus.Completed))
                {
                    // Update the project status
                    w.Project.Status = ProjectStatus.Completed;

                    await Database.SaveChangesAsync(c);

                    Console.WriteLine($"  ⮑  Updated project {w.Project.Name} to completed");
                }
                else
                {
                    Console.WriteLine($"  ⮑  Project {w.Project.Name} still has incomplete tasks");
                }

                _processed.Add(w.Id);
            }
        );
    }
}
