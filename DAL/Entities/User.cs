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
        public virtual Avatar? Avatar { get; set; }
        public virtual ICollection<UserSession>? Sessions { get; set; }
        //TODO: добавить дополнительные поля:
        // string Fullname
        // string Bio
        // string Phone
        // bool IsPhineConfirmed
        // bool IsEmailConfirmed
        // bool PrivateAccount
        // ICollection<Guid> followers
        // ICollection<Guid> following
        // bool isActive
    }
}
