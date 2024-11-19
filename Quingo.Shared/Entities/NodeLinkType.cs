namespace Quingo.Shared.Entities
{
    public class NodeLinkType : EntityBase, IPackOwned
    {
        public string? Name { get; set; }

        public int PackId { get; set; }

        public Pack Pack { get; set; } = default!;

        public List<NodeLink> NodeLinks { get; set; } = [];
    }
}
