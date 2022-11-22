using System.ComponentModel.DataAnnotations;

namespace Api.Models.Comment
{
    public class CreateComment
    {
        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Длина комментария должна быть от 1 до 1000 символов")]
        public string CommentText { get; set; } = null!;
        public Guid PostId { get; set; }
    }
}
