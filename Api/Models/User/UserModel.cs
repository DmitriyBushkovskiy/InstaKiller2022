namespace Api.Models.User
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }
        public int PostsAmount { get; set; }
        public int FollowedAmount { get; set; }
        public int FollowersAmount { get; set; }
    }
}
