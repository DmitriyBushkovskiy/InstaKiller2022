namespace Api.Models
{
    public class PostContentModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public string connectLink { get; set; } = null!;
    }
}
