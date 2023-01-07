using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "empty";
        public string Email { get; set; } = "empty";
        public string PasswordHash { get; set; } = "empty"; 
        public DateTimeOffset BirthDate { get; set; }
        public DateTimeOffset Registered { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool PrivateAccount { get; set; }
        public bool IsActive { get; set; } = true;
        public bool colorAvatar { get; set; } = false;
        public virtual Avatar? Avatar { get; set; }
        public string? PushToken { get; set; }
        public virtual ICollection<UserSession> Sessions { get; set; } = null!;
        public virtual ICollection<Post> Posts { get; set; } = null!;
        public virtual ICollection<Relation> Followers { get; set; } = null!;
        public virtual ICollection<Relation> Followed { get; set; } = null!;
    }
}
