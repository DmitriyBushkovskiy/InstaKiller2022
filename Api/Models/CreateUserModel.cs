using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CreateUserModel
    {
        [Required] //TODO: добавить валидацию (продумать какие символы можно использовать)
        public string Username { get; set; }
        [Required] //TODO: добавить валидацию
        public string Email { get; set; } 
        [Required] //TODO: добавить валидацию (продумать какие символы можно использовать, длина пароля, верхний и нижний регистры)
        public string Password { get; set; }
        [Required]
        [Compare(nameof(Password))]
        public string RetryPassword { get; set; } 
        [Required]
        public DateTimeOffset BirthDate { get; set; } //TODO: добавить валидацию (ограничение на минимальный возраст)

        public CreateUserModel(string username, string email, string password, string retryPassword, DateTimeOffset birthDate)
        {
            Username = username;
            Email = email;
            Password = password;
            RetryPassword = retryPassword;
            BirthDate = birthDate;
        }   
    }
}
