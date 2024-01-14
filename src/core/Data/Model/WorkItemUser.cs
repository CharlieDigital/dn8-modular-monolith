using Microsoft.EntityFrameworkCore;

namespace ChrlsChn.MoMo.Data.Model;

[PrimaryKey(nameof(WorkItemId), nameof(UserId))]
public class WorkItemUser {
  public Guid WorkItemId { get; set; }

  public Guid UserId { get; set; }
}
