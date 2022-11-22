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
