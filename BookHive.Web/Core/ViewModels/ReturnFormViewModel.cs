using BookHive.Web.consts;
using CloudinaryDotNet.Actions;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace BookHive.Web.Core.ViewModels
{
    public class ReturnFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Penalty Paid?")]
        [AssertThat("(TotalDelaysIndays == 0 && PenaltyPaid == false) || PenaltyPaid == true", ErrorMessage = Validationscs.PenaltyShouldBePaid)]
        public bool PenaltyPaid { get; set; }

        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();

        public List<ReturnCopyViewModel> SelectedCopies { get; set; } = new List<ReturnCopyViewModel>();

        public bool AllowExtend { get; set; }

        public int TotalDelaysIndays { 
            get
            {
                return Copies.Sum(x=>x.DelayInDays);
            }
        }
    }
}
