using DAL.Entities;

namespace Api.Models.User
{
    public class UserDataModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
    }
}
