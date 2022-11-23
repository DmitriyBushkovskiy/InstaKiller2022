using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Chat
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPrivate { get; set; }
        public Guid CreatorId { get; set; }
        public virtual List<Message> Messages { get; set; } = new List<Message>();
        public virtual List<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public virtual User Creator { get; set; } = null!;
    }
}
