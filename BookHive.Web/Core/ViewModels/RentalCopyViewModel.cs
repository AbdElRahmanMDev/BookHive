using BookHive.Web.Core.Enums;
using System.Reflection;

namespace BookHive.Web.Core.ViewModels
{
    //RentalCopy is a bridge with meaningful data about how a specific book copy was used in a specific rental.
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }

        public RentalViewModel? Rentals { get; set; }
        public DateTime RentalDate { get; set; } 

        public DateTime EndDate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }

        public int DelayInDays { 
            
            get
            {
                var delay = 0;
                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    delay = (int)(ReturnDate.Value - EndDate).TotalDays;

                else if(!ReturnDate.HasValue && DateTime.Today > EndDate)
                    delay = (int)(DateTime.Today - EndDate).TotalDays;

                return delay;
            }
                
         }
    }
}
