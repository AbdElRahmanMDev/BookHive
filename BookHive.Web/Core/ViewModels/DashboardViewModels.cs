namespace BookHive.Web.Core.ViewModels
{
    public class DashboardViewModels
    {
        public int NumberOfCopies { get; set; }

        public int NumberOfSubscribers { get; set; }

        public IEnumerable<BookViewModel> LastAddedBooks { get; set; } = new List<BookViewModel>();
        public IEnumerable<BookViewModel> TopBooks { get; set; } = new List<BookViewModel>();
    }
}
