namespace Api.Models.Post
{
    public class GetPostsRequestModel
    {
        public Guid? UserId { get; set; }
        public String? LastPostDate { get; set; } = null!;
        public int postsAmount { get; set; }
    }
}
