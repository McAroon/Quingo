namespace Quingo.Shared.Entities
{
    public class Node : EntityBase
    {
        public int PackId { get; set; }

        public string? Name { get; set; }

        public string? ImageUrl { get; set; }

        public List<NodeLink> NodeLinksFrom { get; } = [];

        public List<NodeLink> NodeLinksTo { get; } = [];

        public List<NodeTag> NodeTags { get; } = [];

        public required Pack Pack { get; set; }

        public List<NodeLink> NodeLinks => [.. NodeLinksFrom, .. NodeLinksTo];

        public List<Node> LinkedNodes => NodeLinks.Select(x => x.NodeFrom).Concat(NodeLinks.Select(x => x.NodeTo)).Where(x => x != this).ToList();
    }
}
