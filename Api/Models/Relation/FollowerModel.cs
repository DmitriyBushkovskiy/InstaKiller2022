using Api.Models.User;

namespace Api.Models.Relation
{
    public class FollowerModel
    {
        public Guid Id { get; set; }
        public bool State { get; set; }
        public virtual UserWithAvatarLinkModel Follower { get; set; } = null!;
    }
}
