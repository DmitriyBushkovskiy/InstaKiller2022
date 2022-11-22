using System.ComponentModel.DataAnnotations;

namespace Api.Models.Comment
{
    public class ChangeCommentModel
    {
        public Guid CommentId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Длина комментария должна быть от 1 до 1000 символов")]
        public string CommentText { get; set; } = null!;
    }
}
