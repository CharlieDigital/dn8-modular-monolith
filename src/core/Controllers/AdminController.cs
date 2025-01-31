using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Controllers;

/// <summary>
/// We identify a group name for these API routes so we can publish two different
/// sets of docs from `SetupSwagger.cs`
/// </summary>
[ApiController]
public class AdminController(ILogger<AdminController> logger, TaskDatabase database)
{
    // 👇 Note here we are specifically identify a group name
    [ApiExplorerSettings(GroupName = Constants.AdminApiGroup)]
    [HttpDelete("/api/users/delete/{userId}", Name = nameof(DeleteUser))]
    public async Task DeleteUser(Guid userId)
    {
        logger.LogDebug("[ADMIN] Deleting user");

        await database.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();
    }

    // 👇 Note here we are specifically identify a group name
    [ApiExplorerSettings(GroupName = Constants.AdminApiGroup)]
    [HttpDelete("/api/project/delete/{projectId}", Name = nameof(DeleteProject))]
    public async Task DeleteProject(Guid projectId)
    {
        logger.LogDebug("[ADMIN] Deleting project");

        await database.Projects.Where(p => p.Id == projectId).ExecuteDeleteAsync();
    }

    // 👇 Note here we are specifically identify a group name
    [ApiExplorerSettings(GroupName = Constants.AdminApiGroup)]
    [HttpDelete("/api/tasks/delete/{workItemId}", Name = nameof(DeleteWorkItem))]
    public async Task DeleteWorkItem(Guid workItemId)
    {
        logger.LogDebug("[ADMIN] Deleting work item");

        await database.WorkItems.Where(w => w.Id == workItemId).ExecuteDeleteAsync();
    }
}
