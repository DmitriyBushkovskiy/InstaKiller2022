using Api.Models.User;

namespace Api.Models.Relation
{
    public class RelationsModel
    {
        public Guid TargetUserId { get; set; }
        public virtual UserWithAvatarLinkModel TargetUser { get; set; } = null!;
        public string RelationAsFollower { get; set; } = null!;
        public string RelationAsFollowed { get; set; } = null!;
    }
}
