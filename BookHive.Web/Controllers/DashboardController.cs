namespace BookHive.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        public DashboardController(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            var NumberOfCopies = _context.BookCopies.Count(c => !c.IsDeleted);
            var NumberOfSubscribers = _context.Subscribers.Count(c => !c.IsDeleted);


            NumberOfCopies = NumberOfCopies <= 10 ? NumberOfCopies : NumberOfCopies / 10 * 10;

            var LastAddedBooks = _context.Books
                .Include(x => x.Author)
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Id).Take(8).ToList();
            var TopBooks = _context.RentalCopies.
               Include(x => x.bookCopy).
               ThenInclude(x => x!.Book).
               ThenInclude(x => x!.Author).
               GroupBy(x => new
               {
                   x.bookCopy!.BookId,
                   x.bookCopy!.Book!.Title,
                   x.bookCopy!.Book!.ImageUrlThumb,
                   AuthorName = x.bookCopy!.Book.Author!.Name
               }).
               Select(b => new
               {
                   b.Key.BookId,
                   b.Key.Title,
                   b.Key.ImageUrlThumb,
                   b.Key.AuthorName,
                   Count = b.Count()
               }).
               OrderByDescending(b => b.Count)
               .Take(6).
               Select(x => new BookViewModel
               {
                   Id = x.BookId,
                   Title = x.Title,
                   ImageUrlThumb = x.ImageUrlThumb,
                   Author = x.AuthorName
               })
               .ToList();




            var model = new DashboardViewModels()
            {
                NumberOfCopies = NumberOfCopies,
                NumberOfSubscribers = NumberOfSubscribers,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(LastAddedBooks),
                TopBooks = TopBooks,
            };



            return View(model);
        }

        [AjaxOnly]
        public IActionResult GetRentalsPerDay(DateTime? StartDay = null, DateTime? EndDate = null)
        {
            StartDay ??= DateTime.Today.AddDays(-29);
            EndDate ??= DateTime.Today;


            var data = _context.RentalCopies.
                Where(x => x.RentalDate >= StartDay && x.RentalDate <= EndDate).
                GroupBy(x => new
                {
                    Date = x.RentalDate,
                }).Select(x => new ChartItemViewModel
                {
                    Label = x.Key.Date.ToString("d MMM"),
                    Value = x.Count().ToString(),
                })
                .ToList();




            List<ChartItemViewModel> figures = new List<ChartItemViewModel>();

            for (var day = StartDay; day <= EndDate; day = day.Value.AddDays(1))
            {

                var dayData = data.SingleOrDefault(x => x.Label == day.Value.ToString("d MMM"));

                var item = new ChartItemViewModel()
                {
                    Label = day.Value.ToString("d MMM"),
                    Value = dayData is null ? "0" : dayData.Value,
                };
                figures.Add(item);
            }

            return Ok(figures);
        }


        public IActionResult GetSubscribersPerCity()
        {
            var data = _context.Subscribers.
            Include(x => x.Governorate).
            Where(x => !x.IsDeleted).
            GroupBy(x => new
            {
                GovernorateName = x.Governorate!.Name,

            }).
            Select(b => new ChartItemViewModel
            {
                Label = b.Key.GovernorateName,
                Value = b.Count().ToString()
            }).ToList();

            return Ok(data);


        }


    }
}
