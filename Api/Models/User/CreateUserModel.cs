using Api.Utils;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.User
{
    public class CreateUserModel
    {
        [Required]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Длина username должна быть не менее 1 и не более 30 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Допускается использовать только латинские буквы, цифры, нижние подчеркивания и точки")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string RetryPassword { get; set; }

        [Required]
        [MinAge]
        public DateTimeOffset BirthDate { get; set; }

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
