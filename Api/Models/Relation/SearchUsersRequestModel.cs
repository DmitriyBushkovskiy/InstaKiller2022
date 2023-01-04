using Api.Models.User;

namespace Api.Models.Relation
{
    public class SearchUsersRequestModel
    {
        public string Username { get; set; } = null!;
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
