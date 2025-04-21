using BookHive.Web.consts;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookHive.Web.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public string? key { get; set; }
        [MaxLength(100)]
        [Display(Name ="Full Name")]
        public string FirstName { get; set; } = null!;
        [MaxLength(100)]
        public string LastName { get; set; } = null!;
        [Display(Name ="Date of Birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Validationscs.ErrorDate)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;


        [MaxLength(13)]
        [Display(Name ="National ID")]
        [RegularExpression(@"^[2-3][0-9]{12}$", ErrorMessage = "National ID must start with 2 or 3 and be 14 digits long.")]
        [Remote(action: "UniqueNationalId", null!, AdditionalFields = "key", ErrorMessage = Validationscs.UniqueErrorMessage)]

        public string NationalId { get; set; } = null!;
        [MaxLength(15)]
        [RegularExpression(RegexPatterns.MobileNumber,ErrorMessage = Validationscs.InvalidMobileNumber)]
        [Remote(action: "UniqueMobileNumber", null!, AdditionalFields = "key", ErrorMessage = Validationscs.UniqueErrorMessage)]

        public string MobileNumber { get; set; } = null!;

        [Display(Name ="Has WhatsApp?")]
        public bool HasWhatsApp { get; set; }
        [MaxLength(150)]
        [EmailAddress]
        [Remote(action: "UniqueEmail", null!, AdditionalFields = "key", ErrorMessage = Validationscs.UniqueErrorMessage)]
        public string Email { get; set; } = null!;

        [RequiredIf("key == ''", ErrorMessage = Validationscs.EmptyImage)]
        public IFormFile? Image { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; } 

        [MaxLength(500)]
        public string? ImageThumbnailUrl { get; set; } 

        public int GovernorateId { get; set; }

        public IEnumerable<SelectListItem>? Governorates { get; set; }

        public int AreaId { get; set; }

        public IEnumerable<SelectListItem>? Areas { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = null!;

        //public bool IsBlackListed { get; set; }
    }
}
