namespace Quingo.Shared.Entities
{
    public class NodeLink : EntityBase
    {
        public int NodeFromId { get; set; }

        public int NodeToId { get; set; }

        public Node NodeFrom { get; set; } = null!;

        public Node NodeTo { get; set; } = null!;

        public int NodeLinkTypeId { get; set; }

        public required NodeLinkType NodeLinkType { get; set; }

        public Meta Meta { get; set; } = new();
    }
}
