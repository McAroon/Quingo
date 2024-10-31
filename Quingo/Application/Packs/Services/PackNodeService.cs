using FluentResults;
using Microsoft.EntityFrameworkCore;
using Quingo.Application.Shared.Models;
using Quingo.Infrastructure.Database.Repos;
using Quingo.Infrastructure.Files;
using Quingo.Shared.Entities;

namespace Quingo.Application.Packs.Services;

public class PackNodeService
{
    private readonly PackRepo _repo;
    private readonly FileStoreService _files;

    public PackNodeService(PackRepo repo, FileStoreService files)
    {
        _repo = repo;
        _files = files;
    }

    public async Task<Result> SaveNode(List<LinkedNodeInfoModel> nodeInfos, int packId, int id, NodeModel model)
    {
        await using var context = await _repo.CreateDbContext();
        
        var pack = await context.Packs
            .Include(pack => pack.Tags)
            .Include(pack => pack.NodeLinkTypes)
            .FirstOrDefaultAsync(x => x.Id == packId);
        if (pack == null)
        {
            return Result.Fail("Pack not found");
        }
        if (id == 0)
        {
            var node = new Node
                {
                    Name = model.Name ?? "New Item",
                    Pack = pack
                };
            context.Update(node);
        }
        else
        {
            var node = await context.Nodes
                .Include(x => x.NodeTags)
                .Include(x => x.NodeLinksFrom)
                .Include(x => x.NodeLinksTo)
                .FirstOrDefaultAsync(x => x.PackId == packId && x.Id == id);
            if (node == null)
            {
                return Result.Fail("Item not found");
            }

            if (node.Name != model.Name)
                node.Name = model.Name;

            foreach (var tagId in model.TagIds)
            {
                var nodeTag = node.NodeTags.FirstOrDefault(x => x.TagId == tagId);
                if (nodeTag == null)
                {
                    var tag = pack.Tags.FirstOrDefault(x => x.Id == tagId);
                    if (tag == null)
                    {
                        return Result.Fail("Tag not found");
                    }

                    nodeTag = new NodeTag
                        {
                            Node = node,
                            Tag = tag
                        };
                    node.NodeTags.Add(nodeTag);
                }
                else if (nodeTag.DeletedAt != null)
                {
                    nodeTag.DeletedAt = null;
                    nodeTag.DeletedByUserId = null;
                }
            }

            foreach (var nodeTag in node.NodeTags.Where(x => !model.TagIds.Contains(x.Tag.Id)))
            {
                context.Remove(nodeTag);
            }

            foreach (var linkModel in model.NodeLinks)
            {
                if (linkModel.LinkDirection is NodeLinkDirection.To or NodeLinkDirection.Both)
                {
                    var link = node.NodeLinksFrom.FirstOrDefault(x => x.NodeToId == linkModel.LinkedNodeId);
                    var linkType = pack.NodeLinkTypes.FirstOrDefault(x => x.Id == linkModel.LinkTypeId);
                    if (linkType == null)
                    {
                        return Result.Fail("Link type not found");
                    }

                    var linkedNode = nodeInfos.FirstOrDefault(x => x.Id == linkModel.LinkedNodeId);
                    if (linkedNode == null)
                    {
                        return Result.Fail("Linked Item not found");
                    }

                    if (link == null)
                    {
                        link = new NodeLink
                            {
                                NodeFrom = node,
                                NodeToId = linkedNode.Id,
                                NodeLinkType = linkType
                            };
                        node.NodeLinksFrom.Add(link);
                    }
                    else
                    {
                        if (link.NodeLinkTypeId != linkModel.LinkTypeId)
                        {
                            link.NodeLinkType = linkType;
                        }
                        if (link.DeletedAt != null)
                        {
                            link.DeletedAt = null;
                            link.DeletedByUserId = null;
                        }
                    }
                }

                if (linkModel.LinkDirection is NodeLinkDirection.From or NodeLinkDirection.Both)
                {
                    var link = node.NodeLinksTo.FirstOrDefault(x => x.NodeFromId == linkModel.LinkedNodeId);
                    var linkType = pack.NodeLinkTypes.FirstOrDefault(x => x.Id == linkModel.LinkTypeId);
                    if (linkType == null)
                    {
                        return Result.Fail("Link type not found");
                    }

                    var linkedNode = nodeInfos.FirstOrDefault(x => x.Id == linkModel.LinkedNodeId);
                    if (linkedNode == null)
                    {
                        return Result.Fail("Linked Item not found");
                    }

                    if (link == null)
                    {
                        link = new NodeLink
                            {
                                NodeFromId = linkedNode.Id,
                                NodeTo = node,
                                NodeLinkType = linkType
                            };
                        node.NodeLinksTo.Add(link);
                    }
                    else
                    {
                        if (link.NodeLinkTypeId != linkModel.LinkTypeId)
                        {
                            link.NodeLinkType = linkType;
                        }
                        if (link.DeletedAt != null)
                        {
                            link.DeletedAt = null;
                            link.DeletedByUserId = null;
                        }
                    }
                }
            }

            foreach (var nodeLink in node.NodeLinksFrom)
            {
                var linkModel = model.NodeLinks
                    .FirstOrDefault(x => x.LinkedNodeId == nodeLink.NodeToId && (x.LinkDirection is NodeLinkDirection.Both or NodeLinkDirection.To));
                if (linkModel == null)
                {
                    context.Remove(nodeLink);
                }
            }

            foreach (var nodeLink in node.NodeLinksTo)
            {
                var linkModel = model.NodeLinks
                    .FirstOrDefault(x => x.LinkedNodeId == nodeLink.NodeFromId && (x.LinkDirection is NodeLinkDirection.Both or NodeLinkDirection.From));
                if (linkModel == null)
                {
                    context.Remove(nodeLink);
                }
            }

            if (model.ImageFile != null)
            {
                var filename = await _files.UploadBrowserFile(model.ImageFile);
                node.ImageUrl = filename;
            }
        }

        await context.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result> DeleteNode(int id)
    {
        await using var context = await _repo.CreateDbContext();
        var node = await context.Nodes
            .Include(x => x.NodeLinksFrom)
            .Include(x => x.NodeLinksTo)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (node == null) return Result.Ok();
        
        context.Remove(node);
        foreach (var link in node.NodeLinks)
        {
            context.Remove(link);
        }
        await context.SaveChangesAsync();
        return Result.Ok();
    }
}