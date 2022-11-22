using Api.Models.Comment;
using Api.Models.PostContent;
using Api.Models.User;
using DAL.Entities;

namespace Api.Models.Post
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool Changed { get; set; }
        public int Likes { get; set; }
        public UserWithAvatarLinkModel Author { get; set; } = null!;
        public List<PostContentModel> PostContent { get; set; } = null!;
        public List<CommentModel>? Comments { get; set; }
    }
}
