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

        var links = NodeLinks.SelectMany(n => n.LinkedNode.Tags, (n, t) => (n, t))
            .Select(x => new NodeLinkByTagInfoModel(x.t, x.n.LinkType, x.n.LinkDirection, NodeLinkByTagType.Direct));

        var indirectLinks = node.NodeTags
            .Select(x => x.Tag)
            .SelectMany(x => x.IndirectLinks, (tag, step) => (tag, step, link: step.IndirectLink))
            .Where(x => (x.step.Order == 0 && x.step.TagFrom == x.tag) || (x.step.Order == x.link.Steps.Count - 1 && x.step.TagTo == x.tag && x.link.Direction != NodeLinkDirection.Both))
            .Select(x =>
            {
                var tag = x.link.Direction == NodeLinkDirection.Both ? x.tag : x.step.Order == 0 ? x.link.Steps.Last().TagTo : x.link.Steps.First().TagFrom;
                var direction = x.link.Direction == NodeLinkDirection.Both ? NodeLinkDirection.Both : x.step.Order == 0 ? NodeLinkDirection.To : NodeLinkDirection.From;
                return new NodeLinkByTagInfoModel(new EntityInfoModel(tag.Id, tag.Name), new EntityInfoModel(x.link.Id, x.link.Name), direction, NodeLinkByTagType.Indirect);
            });

        NodeLinksByTag = links.Concat(indirectLinks)
            .GroupBy(x => (tag: x.Tag.Id, name: x.LinkType.Name, dir: x.LinkDirection, type: x.Type))
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

public class NodeLinkByTagInfoModel(EntityInfoModel tag, EntityInfoModel linkType, NodeLinkDirection linkDirection, NodeLinkByTagType type)
{
    public EntityInfoModel Tag { get; set; } = tag;

    public EntityInfoModel LinkType { get; set; } = linkType;

    public NodeLinkDirection LinkDirection { get; set; } = linkDirection;

    public NodeLinkByTagType Type { get; set; } = type;
}

public enum NodeLinkByTagType
{
    Direct,
    Indirect
}

public class IndirectLinkModel
{
    public string? Name { get; set; }

    public NodeLinkDirection Direction { get; set; } = NodeLinkDirection.To;

    public List<IndirectLinkStepModel> Steps { get; set; } = [];

    public IndirectLinkModel()
    {
        
    }

    public IndirectLinkModel(IndirectLink link)
    {
        Name = link.Name;
        Direction = link.Direction;
        Steps = link.Steps.Select(x => new IndirectLinkStepModel(x)).ToList();
    }
}

public class IndirectLinkStepModel
{
    public int? TagFromId { get; set; }

    public int? TagToId { get; set; }

    public IndirectLinkStepModel()
    {
        
    }

    public IndirectLinkStepModel(IndirectLinkStep step)
    {
        TagFromId = step.TagFromId;
        TagToId = step.TagToId;
    }
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
