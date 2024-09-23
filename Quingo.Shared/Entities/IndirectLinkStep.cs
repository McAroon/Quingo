namespace Quingo.Shared.Entities;

public class IndirectLinkStep : EntityBase
{
    public int Order { get; set; }

    public int IndirectLinkId { get; set; }

    public int TagFromId { get; set; }

    public int TagToId { get; set; }

    public IndirectLink IndirectLink { get; set; } = default!;

    public Tag TagFrom { get; set; } = default!;

    public Tag TagTo { get; set; } = default!;
}
