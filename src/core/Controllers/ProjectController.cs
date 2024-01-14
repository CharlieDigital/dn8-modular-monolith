using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Controllers;

[ApiController]
public class ProjectController(
  ILogger<ProjectController> logger,
  TaskDatabase database
) : ControllerBase {

  [HttpGet("/api/projects/", Name = nameof(GetProjects))]
  public async Task<IEnumerable<Project>> GetProjects() {
    logger.LogDebug("Getting projects");

    return await database.Projects.ToListAsync();
  }

  [HttpPost("/api/projects/add", Name = nameof(AddProject))]
  public async Task<Project> AddProject(
    [FromBody] string name
  ) {
    logger.LogDebug("Adding project");

    var project = new Project {
      Id = Guid.NewGuid(),
      Name = name,
      CreatedUtc = DateTimeOffset.UtcNow,
      Status = ProjectStatus.NotStarted
    };

    await database.AddAsync(project);

    await database.SaveChangesAsync();

    return project;
  }
}
