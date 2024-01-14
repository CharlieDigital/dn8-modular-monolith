using System.Text.Json.Serialization;

namespace ChrlsChn.MoMo.Data.Model;

public class User : EntityBase {
  public required string Email { get; set; }

  [JsonIgnore]
  public List<WorkItem> WorkItems { get; set; } = [];
}
