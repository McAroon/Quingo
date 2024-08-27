namespace Quingo.Shared.Entities
{
    public class Tag : EntityBase
    {
        public string? Name { get; set; }
        public List<NodeTag> NodeTags { get; } = [];

        public int PackId { get; set; }

        public required Pack Pack { get; set; }
    }
}
