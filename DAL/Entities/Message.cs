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
        public Guid RecipientId { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool IsActive { get; set; } = true;
        public bool State { get; set; } = false ;
        virtual public User Author { get; set; } = null!;
        virtual public User Recipient { get; set; } = null!;
    }

    public class Chat
    { 
        public Guid Id { get; set; }

        public DateTimeOffset Created;
        public virtual List<Message> Messages { get; set; } = null!;
    }

    public class PrivateChat : Chat
    {
        public virtual User User1 { get; set; } = null!;
        public virtual User User2 { get; set; } = null!;
    }

    public class GroupChat : Chat
    {
        virtual public List<User> Participants { get; set; } = null!;
        public bool IsPrivate { get; set; }
        public virtual User Creator { get; set; } = null!; 
    }
}
