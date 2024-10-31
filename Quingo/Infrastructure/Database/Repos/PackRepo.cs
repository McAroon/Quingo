using Microsoft.EntityFrameworkCore;
using Quingo.Shared.Entities;

namespace Quingo.Infrastructure.Database.Repos;
public class PackRepo(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<ApplicationDbContext> CreateDbContext()
    {
        return await dbContextFactory.CreateDbContextAsync();
    }

    public async Task<Pack?> GetPack(int packId)
    {
        await using var context = await CreateDbContext();
        var packQ = context.Packs
            .Include(x => x.Tags)
            .Include(x => x.NodeLinkTypes)
            .Include(x => x.Presets)
            .Include(x => x.IndirectLinks).ThenInclude(x => x.Steps)
            .Include(x => x.Nodes).ThenInclude(x => x.NodeTags)
            .Include(x => x.Nodes).ThenInclude(x => x.NodeLinksFrom)
            .Include(x => x.Nodes).ThenInclude(x => x.NodeLinksTo)
            .AsSplitQuery();
        var pack = await packQ.FirstOrDefaultAsync(x => x.Id == packId);
        if (pack == null) return null;

        PopulatePackNodes(pack, pack.Nodes);

        return pack;
    }

    public async Task<Pack?> GetPackExclNodes(int packId)
    {
        await using var context = await CreateDbContext();
        var packQ = context.Packs
            .Include(x => x.Tags)
            .Include(x => x.NodeLinkTypes)
            .Include(x => x.Presets)
            .Include(x => x.IndirectLinks).ThenInclude(x => x.Steps)
            .AsSplitQuery();
        
        var pack = await packQ.FirstOrDefaultAsync(x => x.Id == packId);
        return pack;
    }

    public async Task<PagedResult<Node>> GetPackNodes(Pack pack, 
        int page = 1, int pageSize = 10, 
        string search = "", List<int>? tagIds = null, 
        PackNodesOrderBy orderBy = PackNodesOrderBy.CreatedAt, OrderDirection direction = OrderDirection.Descending)
    {
        await using var context = await CreateDbContext();
        var result = new PagedResult<Node>
        {
            Page = page,
            PageSize = pageSize,
        };
        
        var nodesQ = context.Nodes
            .Where(x => x.PackId == pack.Id)
            .Include(x => x.NodeTags)
            .Include(x => x.NodeLinksFrom)
            .Include(x => x.NodeLinksTo)
            .AsSplitQuery();
        
        nodesQ = OrderNodes(nodesQ, orderBy, direction);

        if (!string.IsNullOrEmpty(search))
        {
            nodesQ = nodesQ.Where(x => EF.Functions.ILike(x.Name ?? "", $"%{search}%"));
        }

        if (tagIds is { Count: > 0 })
        {
            nodesQ = nodesQ.Where(x => x.NodeTags.Any(y => tagIds.Contains(y.TagId)));
        }
        
        result.Count = await nodesQ.CountAsync();
        
        nodesQ = nodesQ.Skip((page - 1) * pageSize).Take(pageSize);
        
        var nodes = await nodesQ.ToListAsync();

        var nodeIds = nodes.Select(x => x.Id).ToList();
        var linkedNodeIds = nodes.SelectMany(x => x.NodeLinks).Select(x => x.NodeFromId)
            .Concat(nodes.SelectMany(x => x.NodeLinks).Select(x => x.NodeToId))
            .Distinct()
            .Where(x => !nodeIds.Contains(x))
            .ToList();
        var linkedNodes = await context.Nodes
            .Include(x => x.NodeTags)
            .Where(x => linkedNodeIds.Contains(x.Id))
            .AsSplitQuery()
            .ToListAsync();
        
        PopulatePackNodes(pack, nodes, linkedNodes);
        
        result.Data = nodes;
        
        return result;
    }

    public async Task<List<(int id, string? name)>> GetPackNodeNames(int packId, string? search = null)
    {
        await using var context = await CreateDbContext();
        var query = context.Nodes
            .Where(x => x.PackId == packId)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name });

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name ?? "", $"%{search}%"));
        }
        
        var result = await query.ToListAsync();
        var resTuples = result.Select(x => (id: x.Id, name: x.Name)).ToList();
        return resTuples;
    }

    private static IOrderedQueryable<Node> OrderNodes(IQueryable<Node> nodes, PackNodesOrderBy orderBy,
        OrderDirection direction)
    {
        return orderBy switch
        {
            PackNodesOrderBy.CreatedAt => direction == OrderDirection.Ascending
                ? nodes.OrderBy(x => x.CreatedAt)
                : nodes.OrderByDescending(x => x.CreatedAt),
            PackNodesOrderBy.Name => direction == OrderDirection.Ascending
                ? nodes.OrderBy(x => x.Name)
                : nodes.OrderByDescending(x => x.Name),
            PackNodesOrderBy.Image => direction == OrderDirection.Ascending
                ? nodes.OrderBy(x => x.ImageUrl)
                : nodes.OrderByDescending(x => x.ImageUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };
    }
    
    private static void PopulatePackNodes(Pack pack, List<Node> nodes, List<Node>? linkedNodes = null)
    {
        foreach (var node in nodes)
        {
            node.Pack ??= pack;
            
            foreach (var tag in node.NodeTags)
            {
                tag.Tag ??= pack.Tags.First(x => x.Id == tag.TagId);
            }

            foreach (var link in node.NodeLinksFrom)
            {
                link.NodeLinkType ??= pack.NodeLinkTypes.First(x => x.Id == link.NodeLinkTypeId);
                link.NodeTo ??= pack.Nodes.FirstOrDefault(x => x.Id == link.NodeToId) 
                                ?? nodes.FirstOrDefault(x => x.Id == link.NodeToId)
                                ?? linkedNodes?.FirstOrDefault(x => x.Id == link.NodeToId)
                                ?? throw new NullReferenceException("Could not find node");
            }

            foreach (var link in node.NodeLinksTo)
            {
                link.NodeLinkType ??= pack.NodeLinkTypes.First(x => x.Id == link.NodeLinkTypeId);
                link.NodeFrom ??= pack.Nodes.FirstOrDefault(x => x.Id == link.NodeFromId)
                                  ?? nodes.FirstOrDefault(x => x.Id == link.NodeFromId)
                                  ?? linkedNodes?.FirstOrDefault(x => x.Id == link.NodeFromId)
                                  ?? throw new NullReferenceException("Could not find node");
            }
        }

        foreach (var link in pack.IndirectLinks)
        {
            link.Steps = [.. link.Steps.OrderBy(x => x.Order)];
            foreach (var step in link.Steps)
            {
                step.TagFrom ??= pack.Tags.First(x => x.Id == step.TagFromId);
                step.TagTo ??= pack.Tags.First(x => x.Id == step.TagToId);
            }
        }

        if (linkedNodes != null)
        {
            foreach (var tag in linkedNodes.SelectMany(node => node.NodeTags))
            {
                tag.Tag ??= pack.Tags.First(x => x.Id == tag.TagId);
            }
        }
    }
}

public enum PackNodesOrderBy
{
    CreatedAt,
    Name,
    Image
}

public enum OrderDirection
{
    Ascending,
    Descending
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
    public List<T> Data { get; set; } = [];
}