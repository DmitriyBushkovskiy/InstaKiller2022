using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public Guid ChatId { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool IsActive { get; set; } = true;
        public bool State { get; set; } = true ;
        virtual public User Author { get; set; } = null!;
    }
}
