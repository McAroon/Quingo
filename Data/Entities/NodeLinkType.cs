namespace Quingo.Data.Entities
{
    public class NodeLinkType : EntityBase
    {
        public string? Name { get; set; }

        public int PackId { get; set; }

        public required Pack Pack { get; set; }

        public List<NodeLink> NodeLinks { get; set; } = [];
    }
}
