using Api.Models.Attach;
using Api.Utils;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.Post
{
    public class CreatePostRequest
    {
        [StringLength(2000, ErrorMessage = "Длина текста должна быть не более 2000 символов")]
        public string? Description { get; set; }

        [Required]
        [NotEmptyListMetadata(ErrorMessage = "Нельзя создать пост без контента")]
        public List<MetadataModel> Content { get; set; } = new List<MetadataModel>();
    }
}
