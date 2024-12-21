namespace Quingo.Shared.Entities
{
    public class Tag : EntityBase, IPackOwned
    {
        public Tag()
        {
            
        }

        public Tag(string name)
        {
            Name = name;
        }

        public string? Name { get; set; }

        public string? Description { get; set; }
        
        public List<NodeTag> NodeTags { get; } = [];

        public int PackId { get; set; }

        public Pack Pack { get; set; } = default!;

        public List<IndirectLinkStep> IndirectLinksFrom { get; set; } = [];

        public List<IndirectLinkStep> IndirectLinksTo { get; set; } = [];

        public List<IndirectLinkStep> IndirectLinks => [.. IndirectLinksFrom, .. IndirectLinksTo];
    }
}
