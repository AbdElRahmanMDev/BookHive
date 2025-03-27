using BookHive.Web.consts;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookHive.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Validationscs.MaxLenErrorMessage)]
        [Remote(action: "check", controller: nameof(Book), AdditionalFields = "Id,AuthorId", ErrorMessage = Validationscs.BookAuthorError)]


        //[RequiredIf("IsAvailableForRental==true",ErrorMessage ="this field is required")]
        public string? Title { get; set; } 
        [Display(Name ="Authors")]
        [Remote(action: "check", controller: nameof(Book), AdditionalFields = "Id,Title", ErrorMessage = Validationscs.BookAuthorError)]
        public int AuthorId { get; set; }

        //Navigation Property It doesn't Exist in the database
        public IEnumerable<SelectListItem>? Authors { get; set; }
        //
        [MaxLength(200,ErrorMessage = Validationscs.MaxLenErrorMessage)] 
        public string Publisher { get; set; } = null!;

        [Display(Name ="Publishing Date")]
        //ExpressiveAnnotation
        [AssertThat("PublishingDate <= Today()",ErrorMessage = Validationscs.ErrorDate)]
        public DateTime PublishingDate { get; set; } =DateTime.Now;
        public string? ImageUrlThumb { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Image { get; set; }

        [MaxLength(50,ErrorMessage = Validationscs.MaxLenErrorMessage)] 
        public string Hall { get; set; } = null!;

        [Display(Name ="Is Avaliable for Rental?")]
        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        [Display(Name ="Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();

        public IEnumerable<SelectListItem>? Categories { get; set; }

    }
}
