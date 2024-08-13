namespace Quingo.Data.Entities
{
    public class Pack : EntityBase
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public List<Node> Nodes { get; } = [];
    }
}
