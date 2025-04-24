using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace BookHive.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        private readonly IDataProtector _dataProtector;

        public HomeController(ILogger<HomeController> logger
            , IApplicationDbContext context
            , IMapper mapper,
             IDataProtectionProvider dataProtector)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
        }

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            var lastAddedBooks = _context.Books.
                        Include(x => x.Author).
                        Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.Id)
                        .Take(10)
                        .ToList();
            var model = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks);


            model.Select(x => x.sKey = _dataProtector.Protect(x.Id.ToString())).ToList();
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode = 500)
        {
            return View(new ErrorViewModel { ErrorCode = statusCode, ErrorDescription = ReasonPhrases.GetReasonPhrase(statusCode) });
        }
    }
}
