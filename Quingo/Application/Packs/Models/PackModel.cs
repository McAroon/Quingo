using Quingo.Shared.Entities;
using System.ComponentModel.DataAnnotations;

namespace Quingo.Application.Packs.Models;

public class PackModel
{
    public PackModel()
    {
        
    }

    public PackModel(Pack? pack)
    {
        Name = pack?.Name ?? "";
        Description = pack?.Description ?? "";
        IsPublished = pack?.IsPublished ?? false;
    }

    [Required]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    public bool IsPublished { get; set; }
}
