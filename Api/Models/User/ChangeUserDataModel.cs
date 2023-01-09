using Api.Utils;
using DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace Api.Models.User
{
    public class ChangeUserDataModel
    {
        [Required]
        [MinAge]
        public DateTimeOffset BirthDate { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [StringLength(30, MinimumLength = 1, ErrorMessage = "Длина username должна быть не менее 1 и не более 30 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Допускается использовать только латинские буквы, цифры, нижнее подчеркивание и точки")]
        public string Username { get; set; } = null!;

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина имени должна быть от 3 до 100 символов")]
        public string? FullName { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "Длина текста должна быть от 3 до 200 символов")]
        public string? Bio { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Required]
        public bool PrivateAccount { get; set; }
    }
}
