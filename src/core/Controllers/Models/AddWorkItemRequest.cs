namespace ChrlsChn.MoMo.Controllers.Models;

/// <summary>
/// Request model for adding a work item
/// </summary>
public record AddWorkItemRequest(Guid ProjectId, string Name, List<Guid> Collaborators);
