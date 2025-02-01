using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ChrlsChn.MoMo.Common.Data.Model;

public class WorkItem : EntityBase
{
    public WorkItemStatus Status { get; set; }

    public Guid ProjectId { get; set; }

    [JsonIgnore, NotNull]
    public Project? Project { get; set; }

    [JsonIgnore]
    public List<User> Users { get; set; } = [];
}

public enum WorkItemStatus
{
    NotStarted,
    InProgress,
    Completed
}
