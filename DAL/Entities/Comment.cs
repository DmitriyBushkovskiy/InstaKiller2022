using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string CommentText { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool Changed { get; set; }
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public virtual User Author { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
        public virtual ICollection<CommentLike> Likes { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
