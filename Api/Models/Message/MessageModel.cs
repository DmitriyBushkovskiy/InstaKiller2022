using Api.Models.User;

namespace Api.Models.Message
{
    public class MessageModel
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool State { get; set; } = true;
        public virtual UserWithAvatarLinkModel Author { get; set; } = null!;
    }
}
