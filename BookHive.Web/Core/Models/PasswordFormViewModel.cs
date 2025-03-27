using BookHive.Web.consts;

namespace BookHive.Web.Core.Models
{
    public class PasswordFormViewModel
    {
        public string? Id { get; set; }

        [StringLength(100, ErrorMessage = Validationscs.Password, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(RegexPatterns.Password, ErrorMessage = Validationscs.WeakPassword)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = Validationscs.ConfirmPassword)]
        public string ConfirmPassword { get; set; } = null!;

    }
}
