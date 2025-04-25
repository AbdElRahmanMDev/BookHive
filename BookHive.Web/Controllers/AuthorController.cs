using FluentValidation;
using FluentValidation.AspNetCore;

namespace BookHive.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archieve)]
    public class AuthorController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IValidator<AuthorFormViewModel> _validator;

        public AuthorController(IApplicationDbContext context, IMapper mapper, IValidator<AuthorFormViewModel> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }



        [HttpGet]
        public IActionResult Index()
        {
            List<Author> authors = _context.Authors.AsNoTracking().ToList();
            var AuthorViewModels = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(AuthorViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_form");
        }

        [HttpPost]
        public IActionResult Create(AuthorFormViewModel authorFormView)
        {
            //Incase you use both data annotation and fluent validation 
            //var validator = _validator.Validate(authorFormView);
            //if (!validator.IsValid)
            //{
            //    validator.AddToModelState(ModelState);
            //}
            if (ModelState.IsValid) //When request is send using Ajax Form
            {
                return BadRequest();
            }
            Author author = _mapper.Map<Author>(authorFormView);
            author.CreatedOn = DateTime.Now;
            author.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Authors.Add(author);
            _context.SaveChanges();

            AuthorViewModel authorViewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", authorViewModel);

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Author? author = _context.Authors.Find(id);
            if (author == null)
            {

                return NotFound();
            }
            var authorFormViewModel = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_form", authorFormViewModel);
        }

        [HttpPost]

        public IActionResult Edit(AuthorFormViewModel authorFormView)
        {
            var author = _context.Authors.FirstOrDefault(x => x.Id == authorFormView.Id);
            if (author == null)
            {
                return NotFound();
            }
            author = _mapper.Map(authorFormView, author);
            author.LastUpdateOn = DateTime.Now;
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            AuthorViewModel authorViewModel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", authorViewModel);
        }

        public IActionResult Toggle_State(int id)
        {
            Author? author = _context.Authors.Find(id);
            if (author == null)
            {
                return NotFound();
            }
            author.LastUpdateOn = DateTime.Now;
            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            return Ok(author.LastUpdateOn.ToString());
        }

        public IActionResult check(AuthorFormViewModel authorViewModel)
        {
            var author = _context.Authors.SingleOrDefault(x => x.Name == authorViewModel.Name);

            var IsValid = author is null || author.Id.Equals(authorViewModel.Id);

            return Json(IsValid);

        }
    }
}
