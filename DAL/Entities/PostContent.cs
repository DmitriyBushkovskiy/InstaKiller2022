using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class PostContent : Attach
    {
        public Guid PostID { get; set; }
        public virtual Post Post { get; set; } = null!;
        public virtual ICollection<ContentLike> Likes { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}