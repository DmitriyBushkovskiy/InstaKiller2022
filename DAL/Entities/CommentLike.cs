using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CommentLike : Like
    {
        public Guid CommentId { get; set; }
        public virtual Comment Comment { get; set; } = null!;
    }
}
