namespace Quingo.Data.Entities
{
    public class Tag : EntityBase
    {
        public required string Name { get; set; }
        public List<NodeTag> NodeTags { get; } = [];
    }
}
