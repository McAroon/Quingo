namespace Quingo.Data.Entities
{
    public class Pack : EntityBase
    {
        public required string Name { get; set; }
        public List<Node> Nodes { get; } = [];
    }
}
