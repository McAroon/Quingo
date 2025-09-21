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
        private readonly ICacheService _cache;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor contextAccessor, ICacheService cache) : base(options)
        {
            _httpContextAccessor = contextAccessor;
            _cache = cache;
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
        public DbSet<TournamentLobby> TournamentLobbies { get; set; } = default!;
        public DbSet<LobbyParticipant> LobbyParticipants { get; set; } = default!;
        public DbSet<TournamentResult> TournamentResults { get; set; } = null!;
        public DbSet<UserPackPreset> UserPackPresets { get; set; } = default!;
        public DbSet<LobbyBan> LobbyBans { get; set; } = default!;

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
            new EntityBaseConfiguration<TournamentLobby>().Configure(builder.Entity<TournamentLobby>());
            new EntityBaseConfiguration<LobbyParticipant>().Configure(builder.Entity<LobbyParticipant>());
            new EntityBaseConfiguration<TournamentResult>().Configure(builder.Entity<TournamentResult>());
            new EntityBaseConfiguration<UserPackPreset>().Configure(builder.Entity<UserPackPreset>());
            new EntityBaseConfiguration<LobbyBan>().Configure(builder.Entity<LobbyBan>());

            builder.Entity<LobbyParticipant>()
                .HasIndex(p => new { p.TournamentLobbyId, p.CreatedAt });

            builder.Entity<TournamentResult>()
                .HasIndex(r => new { r.LobbyId, r.UpdatedAt, r.CreatedAt });

            builder.Entity<TournamentLobby>()
                .HasIndex(l => l.UpdatedAt);

            builder.Entity<LobbyBan>()
                .HasOne(b => b.Lobby)
                .WithMany(l => l.Bans)
                .HasForeignKey(b => b.TournamentLobbyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<LobbyBan>()
                .HasIndex(b => new { b.TournamentLobbyId, b.UserId })
                .IsUnique();

            builder.Entity<UserPackPreset>()
                .OwnsOne(e => e.Data, d =>
                {
                    d.ToJson();
                    d.OwnsMany(x => x.Columns, c =>
                    {
                        c.OwnsMany(x => x.ColAnswerTags);
                    });
                });

            builder.Entity<UserPackPreset>()
                .HasIndex(x => new { x.UserId, x.PackId, x.TournamentMode })
                .IsUnique();

            builder.Entity<TournamentLobby>()
                .HasMany(x => x.Participants)
                .WithOne(x => x.Lobby)
                .HasForeignKey(x => x.TournamentLobbyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TournamentLobby>()
                .OwnsOne(e => e.PresetData, d =>
                {
                    d.ToJson();
                    d.OwnsMany(x => x.Columns, c =>
                    {
                        c.OwnsMany(x => x.ColAnswerTags);
                    });
                });

            builder.Entity<LobbyParticipant>()
                .HasOne(x => x.Lobby)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.TournamentLobbyId);

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
            builder.Entity<Node>().HasIndex(e => e.Difficulty);
            builder.Entity<Node>().Property(x => x.Difficulty).HasDefaultValue(1);

            builder.Entity<Tag>().Ignore(e => e.IndirectLinks);

            builder.Entity<NodeLink>().HasOne(e => e.NodeFrom).WithMany(e => e.NodeLinksFrom)
                .HasForeignKey(e => e.NodeFromId).IsRequired();
            builder.Entity<NodeLink>().HasOne(e => e.NodeTo).WithMany(e => e.NodeLinksTo).HasForeignKey(e => e.NodeToId)
                .IsRequired();
            builder.Entity<NodeLink>().OwnsOne(e => e.Meta, d =>
            {
                d.ToJson();
                d.Ignore(x => x.PropertiesDict);
                d.OwnsMany(x => x.Properties);
            });

            builder.Entity<NodeTag>().HasOne(e => e.Node).WithMany(e => e.NodeTags).HasForeignKey(e => e.NodeId)
                .IsRequired();
            builder.Entity<NodeTag>().HasOne(e => e.Tag).WithMany(e => e.NodeTags).HasForeignKey(e => e.TagId)
                .IsRequired();

            builder.Entity<Pack>().HasMany(e => e.Nodes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.NodeLinkTypes).WithOne(e => e.Pack).HasForeignKey(e => e.PackId)
                .IsRequired();
            builder.Entity<Pack>().HasMany(e => e.Tags).WithOne(e => e.Pack).HasForeignKey(e => e.PackId).IsRequired();
            builder.Entity<Pack>().HasMany(e => e.Presets).WithOne(e => e.Pack).HasForeignKey(e => e.PackId)
                .IsRequired();
            builder.Entity<Pack>().HasMany(e => e.IndirectLinks).WithOne(e => e.Pack).HasForeignKey(e => e.PackId)
                .IsRequired();

            builder.Entity<PackPreset>().OwnsOne(e => e.Data, d =>
            {
                d.ToJson();
                d.OwnsMany(x => x.Columns, c =>
                {
                    c.OwnsMany(x => x.ColAnswerTags);
                });
            });

            builder.Entity<IndirectLink>().Property(e => e.Direction).HasConversion<string>();
            builder.Entity<IndirectLink>().HasMany(e => e.Steps).WithOne(e => e.IndirectLink)
                .HasForeignKey(e => e.IndirectLinkId).IsRequired();
            builder.Entity<IndirectLink>().Ignore(e => e.TagFrom);
            builder.Entity<IndirectLink>().Ignore(e => e.TagTo);

            builder.Entity<IndirectLinkStep>().HasOne(e => e.TagTo).WithMany(e => e.IndirectLinksTo)
                .HasForeignKey(e => e.TagToId).IsRequired();
            builder.Entity<IndirectLinkStep>().HasOne(e => e.TagFrom).WithMany(e => e.IndirectLinksFrom)
                .HasForeignKey(e => e.TagFromId).IsRequired();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetFieldsOnSave();
            ClearCacheOnSave();
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
                        if (entry.Entity is LobbyParticipant
                            || entry.Entity is TournamentLobby
                            || entry.Entity is LobbyBan)
                            continue;

                        entity.DeletedAt = DateTime.UtcNow;
                        if (userId != null)
                        {
                            entity.DeletedByUserId = userId;
                        }

                        entry.State = EntityState.Modified;

                        if (entry.Entity is IHasMeta { Meta.Properties: not null } metaEntity)
                        {
                            var metaEntry = Entry(metaEntity.Meta);
                            metaEntry.State = EntityState.Unchanged;
                            var propsEntry = metaEntry.Collection(c => c.Properties);
                            foreach (var prop in metaEntity.Meta.Properties)
                            {
                                propsEntry.FindEntry(prop)!.State = EntityState.Unchanged;
                            }
                        }
                    }
                }
            }
        }

        private void ClearCacheOnSave()
        {
            var entries = ChangeTracker.Entries().Where(x => x.Entity is Pack or IPackOwned);
            var packIds = entries.Select(x => x.Entity switch
                {
                    Pack p => p.Id,
                    IPackOwned po => po.PackId,
                    _ => (int?)null
                })
                .Where(x => x != null)
                .Select(x => x!.Value)
                .Distinct();
            foreach (var packId in packIds)
            {
                var key = $"pack:{packId}";
                _cache.Remove(key);
            }
        }

        /*
        CREATE EXTENSION IF NOT EXISTS pg_trgm;
        CREATE EXTENSION IF NOT EXISTS unaccent;

        CREATE OR REPLACE FUNCTION public.immutable_unaccent(regdictionary, text)
          RETURNS text
          LANGUAGE c IMMUTABLE PARALLEL SAFE STRICT AS
        '$libdir/unaccent', 'unaccent_dict';

        CREATE OR REPLACE FUNCTION public.f_unaccent(text)
          RETURNS text
          LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
        RETURN public.immutable_unaccent(regdictionary 'public.unaccent', $1);

        create index "IX_Nodes_Name_Unaccent" on "Nodes" using gin (public.f_unaccent("Name") gin_trgm_ops);
        */
        [DbFunction("f_unaccent", "public")]
        public static string FUnaccent(string? text)
        {
            throw new NotImplementedException();
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