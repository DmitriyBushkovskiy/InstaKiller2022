using System.ComponentModel.DataAnnotations;

namespace Api.Models.User
{
    public class ChangeUsernameModel
    {
        [StringLength(30, MinimumLength = 1, ErrorMessage = "Длина username должна быть не менее 1 и не более 30 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Допускается использовать только латинские буквы, цифры, нижнее подчеркивание и точки")]
        public string Username { get; set; } = null!;
    }
}
