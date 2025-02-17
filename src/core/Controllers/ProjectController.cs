using ChrlsChn.MoMo.Common.Data;
using ChrlsChn.MoMo.Common.Data.Model;
using ChrlsChn.MoMo.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Controllers;

[ApiController]
public class ProjectController(ILogger<ProjectController> logger, TaskDatabase database)
    : ControllerBase
{
    [ApiExplorerSettings(GroupName = Constants.DefaultApiGroup)]
    [HttpGet("/api/projects/", Name = nameof(GetProjects))]
    public async Task<IEnumerable<Project>> GetProjects()
    {
        logger.LogInformation("[PROJECT] Getting projects");

        return await database.Projects.ToListAsync();
    }

    [ApiExplorerSettings(GroupName = Constants.DefaultApiGroup)]
    [HttpPost("/api/projects/add", Name = nameof(AddProject))]
    public async Task<Project> AddProject([FromBody] string name)
    {
        logger.LogInformation("[PROJECT] Adding project");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedUtc = DateTimeOffset.UtcNow,
            CurrentStatus = ProjectStatus.NotStarted
        };

        await database.AddAsync(project);

        await database.SaveChangesAsync();

        return project;
    }
}
