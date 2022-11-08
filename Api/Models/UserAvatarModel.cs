namespace Api.Models
{
    public class UserAvatarModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }
        public string? Avarar { get; set; }
    }
}
