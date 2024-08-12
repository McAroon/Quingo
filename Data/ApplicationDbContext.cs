using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quingo.Data.Entities;

namespace Quingo.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeLink> NodeLinks { get; set; }
        public DbSet<NodeTag> NodeTags { get; set; }
        public DbSet<Pack> Packs { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            new EntityBaseConfiguration<Node>().Configure(builder.Entity<Node>());
            new EntityBaseConfiguration<NodeLink>().Configure(builder.Entity<NodeLink>());
            new EntityBaseConfiguration<NodeTag>().Configure(builder.Entity<NodeTag>());
            new EntityBaseConfiguration<Pack>().Configure(builder.Entity<Pack>());
            new EntityBaseConfiguration<Tag>().Configure(builder.Entity<Tag>());

            builder.Entity<Node>().Ignore(e => e.NodeLinks);
            builder.Entity<Node>().Ignore(e => e.LinkedNodes);

            builder.Entity<NodeLink>().HasOne(e => e.Node1).WithMany(e => e.NodeLinks1).HasForeignKey(e => e.Node1Id).IsRequired();
            builder.Entity<NodeLink>().HasOne(e => e.Node2).WithMany(e => e.NodeLinks2).HasForeignKey(e => e.Node2Id).IsRequired();

            builder.Entity<NodeTag>().HasOne(e => e.Node).WithMany(e => e.NodeTags).HasForeignKey(e => e.NodeId).IsRequired();
            builder.Entity<NodeTag>().HasOne(e => e.Tag).WithMany(e => e.NodeTags).HasForeignKey(e => e.TagId).IsRequired();

            builder.Entity<Pack>().HasMany(e => e.Nodes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
        }
    }

    public class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : EntityBase
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CreatedAt).ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);
            builder.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate()
                .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Save);
        }
    }
}
