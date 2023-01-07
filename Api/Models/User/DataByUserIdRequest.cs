namespace Api.Models.User
{
    public class DataByUserIdRequest
    {
        public Guid UserId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
