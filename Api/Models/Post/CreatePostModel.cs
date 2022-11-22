using Api.Models.Attach;
using Api.Utils;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Post
{
    public class CreatePostModel
    {
        public Guid AuthorId { get; set; }

        public string? Description { get; set; }

        public List<MetadataModel> Content { get; set; } = null!;
    }
}

