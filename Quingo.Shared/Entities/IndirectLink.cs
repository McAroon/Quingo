namespace Quingo.Shared.Entities;

public class IndirectLink : EntityBase
{
    public int PackId { get; set; }

    public string? Name { get; set; }

    public int? TagFromId { get; set; }

    public int? TagToId { get; set; }

    public Pack Pack { get; set; } = default!;

    public Tag TagFrom { get; set; } = default!;

    public Tag TagTo { get; set; } = default!;
}
