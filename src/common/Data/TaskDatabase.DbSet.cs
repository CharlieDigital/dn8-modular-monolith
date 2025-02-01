using System.Diagnostics.CodeAnalysis;
using ChrlsChn.MoMo.Common.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Common.Data;

/// <summary>
/// Partial class for the database which contains the main setup code.
/// </summary>
public partial class TaskDatabase : DbContext
{
    [NotNull]
    public DbSet<User>? Users { get; set; }

    [NotNull]
    public DbSet<WorkItem>? WorkItems { get; set; }

    [NotNull]
    public DbSet<Project>? Projects { get; set; }

    [NotNull]
    public DbSet<WorkItemUser>? WorkItemUsers { get; set; }
}
