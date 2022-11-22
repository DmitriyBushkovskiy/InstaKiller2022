using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Relation
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public virtual User Follower { get; set; } = null!;
        public Guid FollowedId { get; set; }
        public virtual User Followed { get; set; } = null!;
        public bool? State { get; set; }
    }
}
