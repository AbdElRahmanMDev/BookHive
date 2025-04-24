using Microsoft.AspNetCore.DataProtection;

namespace BookHive.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IDataProtector _dataProtector;
        private readonly IApplicationDbContext _context;

        private readonly IMapper _mapper;

        public SearchController(IDataProtectionProvider dataProtector, IApplicationDbContext context, IMapper mapper)
        {
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
        {

            return View();
        }


        public IActionResult Find(string query)
        {
            var books = _context.Books.
                Include(x => x.Author)
                .Where(x => !x.IsDeleted && (x.Title.Contains(query) || x.Author!.Name.Contains(query)))
                .Select(x => new
                {
                    Title = x.Title,
                    Name = x.Author!.Name,
                    sKey = _dataProtector.Protect(x.Id.ToString())
                }).ToList();



            return Ok(books);
        }
        public IActionResult Details(string skey)
        {
            var bookId = int.Parse(_dataProtector.Unprotect(skey));

            var book = _context.Books.
                      Include(x => x.Author).
                      Include(x => x.Copies).
                      ThenInclude(x => x.Rentals).
                      Include(x => x.Categories).
                      ThenInclude(x => x.Category).
                      FirstOrDefault(x => x.Id == bookId);
            if (book is null)
                return NotFound();

            var bookViewModel = _mapper.Map<BookViewModel>(book);

            return View(bookViewModel);

        }



    }
}
