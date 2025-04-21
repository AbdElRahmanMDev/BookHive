namespace BookHive.Web.Core.ViewModels
{
    public class RentalHistoryViewModel
    {
        public string Subscriber { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;

        [Display(Name ="Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name ="End Date")]
        public DateTime EndDate { get; set; }

        public DateTime RentalDate { get; set; } 
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendedOn { get; set; }

        public int DelayInDays
        {

            get
            {
                var delay = 0;
                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    delay = (int)(ReturnDate.Value - EndDate).TotalDays;

                else if (!ReturnDate.HasValue && DateTime.Today > EndDate)
                    delay = (int)(DateTime.Today - EndDate).TotalDays;

                return delay;
            }

        }

    }
}
