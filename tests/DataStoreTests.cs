using ChrlsChn.MoMo.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace tests;

public class DataStoreTests : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task Can_Create_Project()
    {
        await using var db = DatabaseFixture.CreateDataStore(false);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 1",
            CreatedUtc = DateTimeOffset.UtcNow,
            Status = ProjectStatus.NotStarted
        };

        await db.Database.BeginTransactionAsync();

        await db.AddAsync(project);

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var loadedProject = await db.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);

        Assert.NotNull(loadedProject);
        Assert.Equal(project.Name, loadedProject.Name);
    }

    [Fact]
    public async Task Can_Create_Project_With_WorkItems()
    {
        await using var db = DatabaseFixture.CreateDataStore(false);

        var now = DateTimeOffset.UtcNow;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 1",
            CreatedUtc = now,
            Status = ProjectStatus.NotStarted,
            WorkItems =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Work Item 1",
                    CreatedUtc = now
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Work Item 2",
                    CreatedUtc = now
                }
            ]
        };

        await db.Database.BeginTransactionAsync();

        await db.AddAsync(project);

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var workItems = await db.WorkItems.Where(w => w.Project.Id == project.Id).ToListAsync();

        Assert.NotEmpty(workItems);
        Assert.Equal(2, workItems.Count);

        var loadedProject = await db
            .Projects.Include(p => p.WorkItems)
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        Assert.NotNull(loadedProject);
        Assert.Equal(project.Name, loadedProject.Name);
        Assert.NotEmpty(project.WorkItems);
        Assert.Equal(2, project.WorkItems.Count);
    }

    [Fact]
    public async Task Can_Create_Project_With_WorkItems_And_Users()
    {
        await using var db = DatabaseFixture.CreateDataStore(false);

        var now = DateTimeOffset.UtcNow;

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Name = "User 1",
            CreatedUtc = now,
            Email = "user1@example.com"
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Name = "User 2",
            CreatedUtc = now,
            Email = "user2@example.com"
        };

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Project 1",
            CreatedUtc = now,
            Status = ProjectStatus.NotStarted,
            WorkItems =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Work Item 1",
                    CreatedUtc = now,
                    Users = [user1, user2]
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Work Item 2",
                    CreatedUtc = now,
                    Users = [user1]
                }
            ]
        };

        await db.Database.BeginTransactionAsync();

        await db.AddAsync(project);

        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();

        var users = await db.Users.ToListAsync();

        Assert.NotEmpty(users);
        Assert.Equal(2, users.Count);

        var workItems = await db
            .WorkItems.Include(w => w.Users)
            .Where(w => w.Project.Id == project.Id)
            .ToListAsync();

        Assert.NotEmpty(workItems);
        Assert.Equal(2, workItems.Count);

        var collaborators = workItems.SelectMany(w => w.Users).Count();

        Assert.Equal(3, collaborators);

        var loadedProject = await db
            .Projects.Include(p => p.WorkItems)
            .FirstOrDefaultAsync(p => p.Id == project.Id);

        Assert.NotNull(loadedProject);
        Assert.Equal(project.Name, loadedProject.Name);
        Assert.NotEmpty(project.WorkItems);
        Assert.Equal(2, project.WorkItems.Count);
    }
}
