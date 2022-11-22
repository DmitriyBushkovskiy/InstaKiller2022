using Api.Models.User;
using DAL.Entities;

namespace Api.Models.Relation
{
    public class FollowedModel
    {
        public Guid Id { get; set; }
        public bool State { get; set; }
        public virtual UserWithAvatarLinkModel Followed { get; set; } = null!;
    }
}
