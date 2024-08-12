namespace Quingo.Data.Entities
{
    public class NodeTag : EntityBase
    {
        public int NodeId { get; set; }

        public int TagId { get; set; }

        public Node Node { get; set; } = null!;

        public Tag Tag { get; set; } = null!;
    }
}
