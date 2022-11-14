using DAL.Entities;

namespace Api.Models.User
{
    public class ChangeUserDataModel
    {
        public DateTimeOffset BirthDate { get; set; } //TODO: валидация 
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
        public bool PrivateAccount { get; set; }
    }
}
