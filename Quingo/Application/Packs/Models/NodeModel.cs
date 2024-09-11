using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Quingo.Application.Packs.Models
{
    public class NodeModel
    {
        [Required]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        public IEnumerable<int> TagIds { get; set; } = [];

        public List<NodeLinkModel> NodeLinks { get; set; } = [];

        public IBrowserFile? ImageFile { get; set; }
    }
}
