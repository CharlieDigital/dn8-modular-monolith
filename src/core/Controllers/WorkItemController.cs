using ChrlsChn.MoMo.Controllers.Models;
using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Controllers;

[ApiController]
public class WorkItemController(
  ILogger<ProjectController> logger,
  TaskDatabase database
) {
  [HttpGet("/api/tasks", Name = nameof(GetWorkItems))]
  public async Task<IEnumerable<WorkItem>> GetWorkItems() {
    logger.LogDebug("Getting tasks");

    return await database.WorkItems.ToListAsync();
  }

  [HttpPost("/api/tasks/add", Name = nameof(AddWorkItem))]
  public async Task<WorkItem> AddWorkItem(AddWorkItemRequest request) {
    logger.LogDebug("Adding task");

    var workItem = new WorkItem {
      Id = Guid.NewGuid(),
      ProjectId = request.ProjectId,
      Name = request.Name,
      Status = WorkItemStatus.NotStarted,
      CreatedUtc = DateTimeOffset.UtcNow
    };

    await database.WorkItems.AddAsync(workItem);

    await database.SaveChangesAsync();

    foreach (var userId in request.Collaborators) {
      await database.WorkItemUsers.AddAsync(new() {
        UserId = userId,
        WorkItemId = workItem.Id
      });
    }

    await database.SaveChangesAsync();

    return workItem;
  }

  [HttpPost("/api/tasks/{taskId}/status", Name = nameof(UpdateStatus))]
  public async Task<WorkItem?> UpdateStatus(
    Guid taskId,
    WorkItemStatus status
  ) {
    logger.LogDebug("Updating task status");

    var task = await database.WorkItems
      .FirstOrDefaultAsync(t => t.Id == taskId);

    if (task == null) {
      return null;
    }

    task.Status = status;
    task.UpdatedUtc = DateTimeOffset.UtcNow;

    await database.SaveChangesAsync();

    return task;
  }
}
