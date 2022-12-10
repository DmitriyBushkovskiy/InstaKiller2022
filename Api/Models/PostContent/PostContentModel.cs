namespace Api.Models.PostContent
{
    public class PostContentModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string ContentLink { get; set; } = null!;
        public int Likes { get; set; }
        public bool LikedByMe { get; set; }
    }
}
