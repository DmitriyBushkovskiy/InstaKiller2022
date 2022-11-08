using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class PostContent : Attach
    {
        public Guid UserID { get; set; }
        public Guid PostID { get; set; }
        public virtual User User { get; set; } = null!;
    }
}