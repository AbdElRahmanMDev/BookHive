using BookHive.Web.consts;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookHive.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }
        [MaxLength(100, ErrorMessage = Validationscs.MaxLenErrorMessage),DisplayName("Full Name")]
        [RegularExpression(RegexPatterns.CharactersOnly_Eng,ErrorMessage =Validationscs.OnlyEnglishLetters)]
        public string FullName { get; set; } = null!;

        [MaxLength(20,ErrorMessage =Validationscs.MaxLenErrorMessage)]
        [Remote(action: "checkUserName", controller: "Users",AdditionalFields ="Id", ErrorMessage = Validationscs.Duplicated)]
        [RegularExpression(RegexPatterns.Username,ErrorMessage =Validationscs.InvalidUsername)]
        public string UserName { get; set; } = null!;
        [MaxLength(200,ErrorMessage =Validationscs.MaxLenErrorMessage),EmailAddress]

        [Remote(action: "checkEmail",controller: "Users",AdditionalFields ="Id",ErrorMessage = Validationscs.Duplicated)]
        public string Email { get; set; } = null!;



        [StringLength(100, ErrorMessage = Validationscs.Password, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(RegexPatterns.Password,ErrorMessage = Validationscs.WeakPassword)]
        [RequiredIf("Id==null")]
        public string? Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = Validationscs.ConfirmPassword)]
        [RequiredIf("Id==null")]
        public string? ConfirmPassword { get; set; } = null!;

        [Display(Name ="Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
