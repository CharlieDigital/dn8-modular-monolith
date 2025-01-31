namespace ChrlsChn.MoMo.Data.Model;

public abstract class EntityBase
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required DateTimeOffset CreatedUtc { get; set; }

    public DateTimeOffset? UpdatedUtc { get; set; }
}
