using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Controllers;

[ApiController]
public class UserController(
  ILogger<ProjectController> logger,
  TaskDatabase database
) {
  [HttpGet("/api/users", Name = nameof(GetUsers))]
  public async Task<IEnumerable<User>> GetUsers() {
    logger.LogDebug("Getting users");

    return await database.Users.ToListAsync();
  }

  [HttpPost("/api/users/add", Name = nameof(AddUser))]
  public async Task<User> AddUser(User user) {
    logger.LogDebug("Adding user");

    await database.Users.AddAsync(user);

    await database.SaveChangesAsync();

    return user;
  }
}
