namespace Quingo.Shared.Entities;

public class IndirectLink : EntityBase
{
    public int PackId { get; set; }

    public string? Name { get; set; }

    public NodeLinkDirection Direction { get; set; }

    public Pack Pack { get; set; } = default!;

    public List<IndirectLinkStep> Steps { get; set; } = [];
}
