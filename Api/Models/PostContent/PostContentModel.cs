namespace Api.Models.PostContent
{
    public class PostContentModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string? ContentLink { get; set; }
        public int Likes { get; set; }
    }
}
