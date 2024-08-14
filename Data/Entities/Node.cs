namespace Quingo.Data.Entities
{
    public class Node : EntityBase
    {
        public int PackId { get; set; }

        public required string Name { get; set; }

        public string? ImageUrl { get; set; }

        public List<NodeLink> NodeLinks1 { get; } = [];

        public List<NodeLink> NodeLinks2 { get; } = [];

        public List<NodeTag> NodeTags { get; } = [];

        public required Pack Pack { get; set; }

        public List<NodeLink> NodeLinks => [.. NodeLinks1, .. NodeLinks2];

        public List<Node> LinkedNodes => NodeLinks.Select(x => x.NodeFrom).Concat(NodeLinks.Select(x => x.NodeTo)).Where(x => x != this).ToList();
    }
}
