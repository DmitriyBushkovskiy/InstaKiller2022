using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class ChatParticipant
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
        public bool? State { get; set; } // true - участник чата, null - отправлена заявка, false - забанен?
        virtual public User User { get; set; } = null!;
        virtual public Chat Chat { get; set; } = null!;
    }
}
