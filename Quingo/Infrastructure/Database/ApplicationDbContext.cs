using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quingo.Shared.Entities;
using System.Security.Claims;

namespace Quingo.Infrastructure.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor contextAccessor) : base(options)
        {
            _httpContextAccessor = contextAccessor;
        }

        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeLink> NodeLinks { get; set; }
        public DbSet<NodeTag> NodeTags { get; set; }
        public DbSet<Pack> Packs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NodeLinkType> NodeLinkTypes { get; set; }
        public DbSet<PackPreset> PackPresets { get; set; }
        public DbSet<IndirectLink> IndirectLinks { get; set; }
        public DbSet<IndirectLinkStep> IndirectLinkSteps { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            new EntityBaseConfiguration<Node>().Configure(builder.Entity<Node>());
            new EntityBaseConfiguration<NodeLink>().Configure(builder.Entity<NodeLink>());
            new EntityBaseConfiguration<NodeTag>().Configure(builder.Entity<NodeTag>());
            new EntityBaseConfiguration<Pack>().Configure(builder.Entity<Pack>());
            new EntityBaseConfiguration<Tag>().Configure(builder.Entity<Tag>());
            new EntityBaseConfiguration<NodeLinkType>().Configure(builder.Entity<NodeLinkType>());
            new EntityBaseConfiguration<PackPreset>().Configure(builder.Entity<PackPreset>());
            new EntityBaseConfiguration<IndirectLink>().Configure(builder.Entity<IndirectLink>());
            new EntityBaseConfiguration<IndirectLinkStep>().Configure(builder.Entity<IndirectLinkStep>());

            builder.Entity<Node>().Ignore(e => e.NodeLinks);
            builder.Entity<Node>().Ignore(e => e.LinkedNodes);
            builder.Entity<Node>().Ignore(e => e.Tags);
            builder.Entity<Node>().OwnsOne(e => e.Meta, d =>
            {
                d.ToJson();
                d.Ignore(x => x.PropertiesDict);
                d.OwnsMany(x => x.Properties);
            });
            builder.Entity<Node>().HasIndex(e => e.Name);
            builder.Entity<Node>().HasIndex(e => e.CreatedAt);
            builder.Entity<Node>().HasIndex(e => e.ImageUrl);

            builder.Entity<Tag>().Ignore(e => e.IndirectLinks);

            builder.Entity<NodeLink>().HasOne(e => e.NodeFrom).WithMany(e => e.NodeLinksFrom).HasForeignKey(e => e.NodeFromId).IsRequired();
            builder.Entity<NodeLink>().HasOne(e => e.NodeTo).WithMany(e => e.NodeLinksTo).HasForeignKey(e => e.NodeToId).IsRequired();
            builder.Entity<NodeLink>().OwnsOne(e => e.Meta, d =>
            {
                d.ToJson();
                d.Ignore(x => x.PropertiesDict);
                d.OwnsMany(x => x.Properties);
            });

            builder.Entity<NodeTag>().HasOne(e => e.Node).WithMany(e => e.NodeTags).HasForeignKey(e => e.NodeId).IsRequired();
            builder.Entity<NodeTag>().HasOne(e => e.Tag).WithMany(e => e.NodeTags).HasForeignKey(e => e.TagId).IsRequired();

            builder.Entity<Pack>().HasMany(e => e.Nodes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.NodeLinkTypes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.Tags).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.Presets).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.IndirectLinks).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();

            builder.Entity<PackPreset>().OwnsOne(e => e.Data, d =>
            {
                d.ToJson();
                d.OwnsMany(x => x.Columns);
            });

            builder.Entity<IndirectLink>().Property(e => e.Direction).HasConversion<string>();
            builder.Entity<IndirectLink>().HasMany(e => e.Steps).WithOne(e => e.IndirectLink).HasForeignKey(e => e.IndirectLinkId).IsRequired();
            builder.Entity<IndirectLink>().Ignore(e => e.TagFrom);
            builder.Entity<IndirectLink>().Ignore(e => e.TagTo);

            builder.Entity<IndirectLinkStep>().HasOne(e => e.TagTo).WithMany(e => e.IndirectLinksTo).HasForeignKey(e => e.TagToId).IsRequired();
            builder.Entity<IndirectLinkStep>().HasOne(e => e.TagFrom).WithMany(e => e.IndirectLinksFrom).HasForeignKey(e => e.TagFromId).IsRequired();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetFieldsOnSave();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetFieldsOnSave()
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is EntityBase entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.UpdatedAt = DateTime.UtcNow;
                        if (userId != null)
                        {
                            entity.CreatedByUserId = userId;
                            entity.UpdatedByUserId = userId;
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                        if (userId != null)
                        {
                            entity.UpdatedByUserId = userId;
                        }
                    }
                    else if (entry.State == EntityState.Deleted && entity.Id > 0)
                    {
                        entity.DeletedAt = DateTime.UtcNow;
                        if (userId != null)
                        {
                            entity.DeletedByUserId = userId;
                        }
                        
                        entry.State = EntityState.Modified;
                    }
                }
            }
        }
    }

    public class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : EntityBase
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId);
            builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserId);
            builder.HasOne(e => e.DeletedByUser).WithMany().HasForeignKey(e => e.DeletedByUserId);
            builder.HasQueryFilter(e => e.DeletedAt == null);

            builder.HasIndex(e => e.DeletedAt);
        }
    }
}
