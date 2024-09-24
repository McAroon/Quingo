namespace Quingo.Shared.Entities;

public class IndirectLink : EntityBase
{
    public int PackId { get; set; }

    public string? Name { get; set; }

    public NodeLinkDirection Direction { get; set; }

    public Pack Pack { get; set; } = default!;

    public List<IndirectLinkStep> Steps { get; set; } = [];

    public Tag? TagFrom => Steps.FirstOrDefault()?.TagFrom;

    public Tag? TagTo => Steps.LastOrDefault()?.TagTo;

    public bool IsLinkedNode(Node node) => node.Tags.Contains(TagFrom) || (node.Tags.Contains(TagTo) && Direction != NodeLinkDirection.Both);
}
