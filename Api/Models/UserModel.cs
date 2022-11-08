namespace Api.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; } 
        public DateTimeOffset BirthDate { get; set; }

        public UserModel (Guid id, string username, string email, DateTimeOffset birthDate)
        {
            Id = id;
            Username = username;
            Email = email;
            BirthDate = birthDate;
        }
    }
}
