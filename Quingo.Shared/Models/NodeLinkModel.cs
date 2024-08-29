using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quingo.Shared.Models
{
    public class NodeLinkModel
    {
        public int? LinkedNodeId => LinkedNode?.Id;

        public NodeInfoModel LinkedNode { get; set; } = default!;

        [Required]
        public int? LinkTypeId { get; set; }

        [Required]
        public NodeLinkDirection LinkDirection { get; set; }
    }
}
