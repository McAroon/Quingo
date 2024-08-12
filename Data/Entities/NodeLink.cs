namespace Quingo.Data.Entities
{
    public class NodeLink : EntityBase
    {
        public int Node1Id { get; set; }

        public int Node2Id { get; set; }

        public Node Node1 { get; set; } = null!;

        public Node Node2 { get; set; } = null!;
    }
}
