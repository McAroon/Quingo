using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using Quingo.Shared.Entities;
using System.Security.Claims;

namespace Quingo.Infrastructure.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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

        public IQueryable<Pack> PacksWithIncludes => Packs
            .Include(x => x.Tags)
            .Include(x => x.NodeLinkTypes)
            .Include(x => x.Presets)
            .Include(x => x.Nodes).ThenInclude(x => x.NodeTags)
            .Include(x => x.Nodes).ThenInclude(x => x.NodeLinksFrom)
            .Include(x => x.Nodes).ThenInclude(x => x.NodeLinksTo);

        public async Task<Pack?> GetPack(int packId, bool noTracking = false)
        {
            var pack = await (noTracking ? PacksWithIncludes.AsNoTracking() : PacksWithIncludes).FirstOrDefaultAsync(x => x.Id == packId);
            if (pack == null) return null;

            foreach (var node in pack.Nodes)
            {
                foreach (var tag in node.NodeTags)
                {
                    tag.Tag ??= pack.Tags.First(x => x.Id == tag.TagId);
                }
                foreach (var link in node.NodeLinksFrom)
                {
                    link.NodeLinkType ??= pack.NodeLinkTypes.First(x => x.Id == link.NodeLinkTypeId);
                    link.NodeTo ??= pack.Nodes.First(x => x.Id == link.NodeToId);
                }
                foreach (var link in node.NodeLinksTo)
                {
                    link.NodeLinkType ??= pack.NodeLinkTypes.First(x => x.Id == link.NodeLinkTypeId);
                    link.NodeFrom ??= pack.Nodes.First(x => x.Id == link.NodeFromId);
                }
            }

            return pack;
        }

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

            builder.Entity<Node>().Ignore(e => e.NodeLinks);
            builder.Entity<Node>().Ignore(e => e.LinkedNodes);

            builder.Entity<NodeLink>().HasOne(e => e.NodeFrom).WithMany(e => e.NodeLinksFrom).HasForeignKey(e => e.NodeFromId).IsRequired();
            builder.Entity<NodeLink>().HasOne(e => e.NodeTo).WithMany(e => e.NodeLinksTo).HasForeignKey(e => e.NodeToId).IsRequired();

            builder.Entity<NodeTag>().HasOne(e => e.Node).WithMany(e => e.NodeTags).HasForeignKey(e => e.NodeId).IsRequired();
            builder.Entity<NodeTag>().HasOne(e => e.Tag).WithMany(e => e.NodeTags).HasForeignKey(e => e.TagId).IsRequired();

            builder.Entity<Pack>().HasMany(e => e.Nodes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.NodeLinkTypes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.Tags).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.Presets).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();

            builder.Entity<PackPreset>().OwnsOne(e => e.Data, d =>
            {
                d.ToJson();
                d.OwnsMany(x => x.Columns);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetFieldsOnSave();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetFieldsOnSave()
        {
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return;
            }

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is EntityBase entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.UpdatedAt = DateTime.UtcNow;
                        entity.CreatedByUserId = userId;
                        entity.UpdatedByUserId = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                        entity.UpdatedByUserId = userId;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        entity.DeletedAt = DateTime.UtcNow;
                        entity.DeletedByUserId = userId;
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
        }
    }
}
