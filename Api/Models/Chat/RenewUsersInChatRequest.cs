namespace Api.Models.Chat
{
    public class RenewUsersInChatRequest
    {
        public List<Guid> TargetUsersId { get; set; } = null!;
        public Guid ChatId { get; set; }
    }
}
