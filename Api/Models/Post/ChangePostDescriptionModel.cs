using System.ComponentModel.DataAnnotations;

namespace Api.Models.Post
{
    public class ChangePostDescriptionModel
    {
        public Guid PostId { get; set; }

        [StringLength(2000, ErrorMessage = "Длина текста должна быть не более 2000 символов")]
        public string? Description { get; set; } = null!;
    }
}
