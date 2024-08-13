namespace Quingo.Data.Entities
{
    public abstract class EntityBase
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? CreatedByUserId { get; set; }

        public string? UpdatedByUserId { get; set; }

        public string? DeletedByUserId { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? UpdatedByUser { get; set; }

        public ApplicationUser? DeletedByUser { get; set; }
    }
}
