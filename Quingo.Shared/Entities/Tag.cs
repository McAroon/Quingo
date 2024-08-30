namespace Quingo.Shared.Entities
{
    public class Tag : EntityBase
    {
        public Tag()
        {
            
        }

        public Tag(string name)
        {
            Name = name;
        }

        public string? Name { get; set; }
        public List<NodeTag> NodeTags { get; } = [];

        public int PackId { get; set; }

        public Pack Pack { get; set; } = default!;
    }
}
