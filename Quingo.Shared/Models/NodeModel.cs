using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quingo.Shared.Models
{
    public class NodeModel
    {
        [Required]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Image Url")]
        public string? ImageUrl { get; set; }

        public IEnumerable<int> TagIds { get; set; } = [];

        public List<NodeLinkModel> NodeLinks { get; set; } = [];
    }
}
