using System.Text.Json.Serialization;

namespace ChrlsChn.MoMo.Common.Data.Model;

public class Project : EntityBase
{
    public required ProjectStatus CurrentStatus { get; set; }

    [JsonIgnore]
    public List<WorkItem> WorkItems { get; set; } = [];
}

public enum ProjectStatus
{
    NotStarted,
    Active,
    Completed
}
