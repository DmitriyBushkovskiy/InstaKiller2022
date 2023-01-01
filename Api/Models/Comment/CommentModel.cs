using Api.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Comment
{
    public class CommentModel
    {
        public Guid Id { get; set; }
        public string CommentText { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool Changed { get; set; }
        public UserWithAvatarLinkModel Author { get; set; } = null!;
        public int Likes { get; set; }
        public bool LikedByMe { get; set; }
    }
}
