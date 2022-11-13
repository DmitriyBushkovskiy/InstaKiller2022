namespace Api.Models.Comment
{
    public class CreateComment
    {
        public string CommentText { get; set; } = null!; //TODO: валидация (не пустая строка и не более?? символов)
        public Guid PostId { get; set; }
    }
}
