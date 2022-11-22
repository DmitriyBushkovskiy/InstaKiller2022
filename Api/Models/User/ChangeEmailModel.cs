using System.ComponentModel.DataAnnotations;

namespace Api.Models.User
{
    public class ChangeEmailModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
