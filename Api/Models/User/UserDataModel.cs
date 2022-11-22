using DAL.Entities;

namespace Api.Models.User
{
    public class UserDataModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
        public bool PrivateAccount { get; set; }
    }
}
