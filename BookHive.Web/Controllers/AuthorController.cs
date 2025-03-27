


using BookHive.Web.Core.Models;
using BookHive.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace BookHive.Web.Controllers
{
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorController(ApplicationDbContext context,IMapper mapper)
        {
            _context= context;
            _mapper= mapper;
        }



        [HttpGet]
        public IActionResult Index()
        {
          List<Author>authors=_context.Authors.AsNoTracking().ToList();
          var  AuthorViewModels=_mapper.Map<IEnumerable<AuthorViewModel>>(authors);
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
            Author author =_mapper.Map<Author>(authorFormView);
            author.CreatedOn = DateTime.Now;
            _context.Authors.Add(author);
            _context.SaveChanges();
            AuthorViewModel authorViewModel=_mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow",authorViewModel);

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Author? author = _context.Authors.Find(id);
            if (author == null)
            {

                return NotFound();
            } 
          var authorFormViewModel=_mapper.Map<AuthorFormViewModel>(author);
          return PartialView("_form", authorFormViewModel);
        }

        [HttpPost]

        public IActionResult Edit(AuthorFormViewModel authorFormView)
        {
            var author=_context.Authors.FirstOrDefault(x=>x.Id== authorFormView.Id);    
            if(author == null)
            {
                return NotFound();
            }
            author=_mapper.Map(authorFormView,author);
            author.LastUpdateOn = DateTime.Now;
            _context.SaveChanges();
            AuthorViewModel authorViewModel=_mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", authorViewModel);
        }

        public IActionResult Toggle_State(int id)
        {
           Author? author=_context.Authors.Find(id);
            if (author == null)
            {
                return NotFound();
            }
            author.LastUpdateOn= DateTime.Now;
            author.IsDeleted=!author.IsDeleted;
            _context.SaveChanges();
            return Ok(author.LastUpdateOn.ToString());
        }

        public IActionResult check(AuthorFormViewModel authorViewModel)
        {
           var author=_context.Authors.SingleOrDefault(x=>x.Name==authorViewModel.Name);

            var IsValid= author is null || author.Id.Equals(authorViewModel.Id);

            return Json(IsValid);

        }
    }
}
