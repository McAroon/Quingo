using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quingo.Shared.Models
{
    public class PackModel
    {
        [Required]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Image Url")]
        public string? ImageUrl { get; set; }
    }
}
