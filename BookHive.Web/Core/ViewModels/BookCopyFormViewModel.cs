using System.ComponentModel;

namespace BookHive.Web.Core.ViewModels
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public bool ShowRentalInput { get; set; }
        [DisplayName("Is Avaliable For Rentals")]
        public bool IsAvailableForRental { get; set; }
        [DisplayName("Edition")]
        [Range(minimum:1,maximum:1000,ErrorMessage ="Edition Number should be between 1 and 1000")]
        public int EditionNumber { get; set; }
    }
}
