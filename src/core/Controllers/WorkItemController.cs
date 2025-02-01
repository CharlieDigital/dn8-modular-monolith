using ChrlsChn.MoMo.Common.Data;
using ChrlsChn.MoMo.Common.Data.Model;
using ChrlsChn.MoMo.Common.Utils;
using ChrlsChn.MoMo.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Controllers;

[ApiController]
public class WorkItemController(ILogger<ProjectController> logger, TaskDatabase database)
{
    [ApiExplorerSettings(GroupName = Constants.DefaultApiGroup)]
    [HttpGet("/api/tasks", Name = nameof(GetWorkItems))]
    public async Task<IEnumerable<WorkItem>> GetWorkItems()
    {
        logger.LogInformation("Getting tasks");

        return await database.WorkItems.ToListAsync();
    }

    [ApiExplorerSettings(GroupName = Constants.DefaultApiGroup)]
    [HttpPost("/api/tasks/add", Name = nameof(AddWorkItem))]
    public async Task<WorkItem> AddWorkItem(AddWorkItemRequest request)
    {
        logger.LogInformation("Adding task");

        var workItem = new WorkItem
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name,
            Status = WorkItemStatus.NotStarted,
            CreatedUtc = DateTimeOffset.UtcNow
        };

        await database.WorkItems.AddAsync(workItem);

        await database.SaveChangesAsync();

        foreach (var userId in request.Collaborators)
        {
            await database.WorkItemUsers.AddAsync(
                new() { UserId = userId, WorkItemId = workItem.Id }
            );
        }

        await database.SaveChangesAsync();

        return workItem;
    }

    [ApiExplorerSettings(GroupName = Constants.DefaultApiGroup)]
    [HttpPost("/api/tasks/{taskId}/status", Name = nameof(UpdateStatus))]
    public async Task<WorkItem?> UpdateStatus(Guid taskId, [FromBody] WorkItemStatus status)
    {
        logger.LogInformation("Updating task status");

        var task = await database.WorkItems.FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return null;
        }

        task.Status = status;
        task.UpdatedUtc = DateTimeOffset.UtcNow;

        await database.SaveChangesAsync();

        return task;
    }
}
