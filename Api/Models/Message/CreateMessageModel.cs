namespace Api.Models.Message
{
    public class CreateMessageModel
    {
        public Guid ChatId { get; set; }
        public string Text { get; set; } = null!; //TODO: validation
    }
}
