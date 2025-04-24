
namespace BookHive.Web.Core.ViewModels
{
    public class RentalViewModel
    {
        public int Id { get; set; }


        public SubscriberViewModel? Subscriber { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;


        public DateTime CreatedOn { get; set; }
        public bool PenaltyPaid { get; set; }

        public IEnumerable<RentalCopyViewModel> RentalCopy { get; set; } = new List<RentalCopyViewModel>();

        public int TotalDelays
        {
            get
            {
                return RentalCopy.Sum(x => x.DelayInDays);
            }
        }
        public int NumberOfCopies
        {
            get
            {
                return RentalCopy.Count();
            }
        }
    }
}
