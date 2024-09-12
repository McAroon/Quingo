using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Quingo.Shared.Entities;

namespace Quingo.Application.Shared.Models;

public class NodeViewModel
{
    public NodeViewModel(Node node)
    {
        var fromIds = node.NodeLinksFrom.Where(x => x.DeletedAt == null).Select(x => (id: x.NodeToId, node: x.NodeTo, linkType: x.NodeLinkType)).ToList();
        var toIds = node.NodeLinksTo.Where(x => x.DeletedAt == null).Select(x => (id: x.NodeFromId, node: x.NodeFrom, linkType: x.NodeLinkType)).ToList();
        var bothIds = fromIds.Join(toIds, x => x.id, y => y.id, (x, y) => x).ToList();
        var linksFrom = fromIds.Except(bothIds).Select(x => new NodeLinkModel
        {
            LinkedNode = new LinkedNodeInfoModel(x.node),
            LinkType = new EntityInfoModel(x.linkType.Id, x.linkType.Name),
            LinkTypeId = x.linkType.Id,
            LinkDirection = NodeLinkDirection.From
        });
        var linksTo = toIds.Except(bothIds).Select(x => new NodeLinkModel
        {
            LinkedNode = new LinkedNodeInfoModel(x.node),
            LinkType = new EntityInfoModel(x.linkType.Id, x.linkType.Name),
            LinkTypeId = x.linkType.Id,
            LinkDirection = NodeLinkDirection.To
        });
        var linksBoth = bothIds.Select(x => new NodeLinkModel
        {
            LinkedNode = new LinkedNodeInfoModel(x.node),
            LinkType = new EntityInfoModel(x.linkType.Id, x.linkType.Name),
            LinkTypeId = x.linkType.Id,
            LinkDirection = NodeLinkDirection.Both
        });

        Id = node.Id;
        Name = node.Name;
        NodeLinks = [.. linksFrom, .. linksTo, .. linksBoth];
        NodeTags = node.NodeTags.Where(x => x.DeletedAt == null).Select(x => new EntityInfoModel(x.Id, x.Tag.Name)).ToList();
        ImageUrl = node.ImageUrl;

        NodeLinksByTag = NodeLinks.SelectMany(n => n.LinkedNode.Tags, (n, t) => (n, t))
            .Select(x => new NodeLinkByTagInfoModel(x.t, x.n.LinkType, x.n.LinkDirection))
            .GroupBy(x => (tag: x.Tag.Id, link: x.LinkType.Id, dir: x.LinkDirection))
            .Select(g => g.First())
            .ToList();
    }

    public NodeViewModel() { }

    public int Id { get; set; }

    public string? Name { get; set; }

    public List<NodeLinkModel> NodeLinks { get; set; } = [];

    public List<EntityInfoModel> NodeTags { get; set; } = [];

    public List<NodeLinkByTagInfoModel> NodeLinksByTag { get; set; } = [];

    public string? ImageUrl { get; set; }
}

public class NodeModel : NodeViewModel
{
    public NodeModel() { }

    public NodeModel(Node node) : base(node)
    {
        TagIds = node.NodeTags.Where(x => x.DeletedAt == null).Select(x => x.TagId).ToList();
    }

    public IEnumerable<int> TagIds { get; set; } = [];

    public IBrowserFile? ImageFile { get; set; }
}

public class NodeLinkModel
{
    public int? LinkedNodeId => LinkedNode?.Id;

    public LinkedNodeInfoModel LinkedNode { get; set; } = default!;

    public EntityInfoModel LinkType { get; set; } = default!;

    public NodeLinkDirection LinkDirection { get; set; }

    public int? LinkTypeId { get; set; }
}

public class EntityInfoModel(int id, string? name)
{
    public int Id { get; set; } = id;

    public string? Name { get; set; } = name;
}

public class LinkedNodeInfoModel(Node node) : EntityInfoModel(node.Id, node.Name)
{
    public IEnumerable<EntityInfoModel> Tags { get; set; } = node.NodeTags.Select(x => new EntityInfoModel(x.TagId, x.Tag.Name));
}

public class NodeLinkByTagInfoModel(EntityInfoModel tag, EntityInfoModel linkType, NodeLinkDirection linkDirection)
{
    public EntityInfoModel Tag { get; set; } = tag;

    public EntityInfoModel LinkType { get; set; } = linkType;

    public NodeLinkDirection LinkDirection { get; set; } = linkDirection;
}

public enum NodeLinkDirection
{
    From,
    To,
    Both
}

public static partial class Extensions
{
    public static string MudIcon(this NodeLinkDirection direction) => direction switch
    {
        NodeLinkDirection.From => Icons.Material.Filled.ChevronLeft,
        NodeLinkDirection.To => Icons.Material.Filled.ChevronRight,
        NodeLinkDirection.Both => Icons.Material.Filled.Code,
        _ => ""
    };
}
