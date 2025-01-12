namespace Quingo.Shared.Entities
{
    public class Pack : EntityBase
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsPublished { get; set; }

        public List<Node> Nodes { get; set; } = [];

        public List<NodeLinkType> NodeLinkTypes { get; set; } = [];

        public List<Tag> Tags { get; set; } = [];

        public List<PackPreset> Presets { get; set; } = [];

        public List<IndirectLink> IndirectLinks { get; set; } = [];
    }
}
