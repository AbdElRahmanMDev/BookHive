using BookHive.Web.Core.Enums;

namespace BookHive.Web.Core.Models
{
    public class RentalCopy
    {
        //RentalCopy is a bridge with meaningful data about how a specific book copy was used in a specific rental.
        public int BookCopyId { get; set; }

        public BookCopy? bookCopy { get; set; }

        public int RentalId { get; set; }

        public Rental? Rentals { get; set; }

        public DateTime RentalDate { get; set; } = DateTime.Today;

        public DateTime? EndDate { get; set; } = DateTime.Today.AddDays((int)RentalConfiguration.RentalDuration);

        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }
    }
}
