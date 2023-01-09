namespace Api.Models.Chat
{
    public class ChatRequestModel
    {
        public Guid ChatId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
