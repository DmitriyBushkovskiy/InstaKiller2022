using DAL.Entities;

namespace Api.Models
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool Changed { get; set; }
        public UserAvatarModel Author { get; set; } = null!;
        public List<PostContentModel> PostContent { get; set; } = null!;
        public List<CommentModel> Comments { get; set; } = null!;

    }
}
