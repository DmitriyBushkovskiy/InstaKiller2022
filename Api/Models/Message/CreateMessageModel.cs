namespace Api.Models.Message
{
    public class CreateMessageModel
    {
        public Guid RecipientId { get; set; }
        public string Text { get; set; } = null!; //TODO: validation
    }
}
