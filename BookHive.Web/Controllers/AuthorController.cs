using BookHive.Application.Common.Interfaces.Repositories;
using BookHive.Application.Services;
using BookHive.Web.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SQLitePCL;

namespace BookHive.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archieve)]
    public class AuthorController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAuthorService _authorService;

        public AuthorController (IMapper mapper,IAuthorService authorService)
        {
            _mapper = mapper;
            _authorService = authorService;
        }



        [HttpGet]
        public IActionResult Index()
        {
            var authors = _authorService.GetAll();
            var AuthorViewModels = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(AuthorViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_form");
        }

        [HttpPost]
        public IActionResult Create(AuthorFormViewModel model)
        {
           
            if (!ModelState.IsValid) //When request is send using Ajax Form
            {
                return BadRequest();
            }

            var author = _authorService.Add(model.Name,User.getUserId());
            AuthorViewModel authorViewModel = _mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", authorViewModel);

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Author? author = _authorService.GetById(id);
            if (author == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_form", model);
        }

        [HttpPost]

        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var author = _authorService.Edit(model.Id, model.Name, User.getUserId());
            AuthorViewModel authorViewModel = _mapper.Map<AuthorViewModel>(author);
            return PartialView("_AuthorRow", authorViewModel);
        }

        public IActionResult Toggle_State(int id)
        {
           var author=_authorService.ToggleStatus(id, User.getUserId());

            if (author is null)
                return NotFound();

            return Ok(author.LastUpdateOn.ToString());
        }

        public IActionResult check(AuthorFormViewModel authorViewModel)
        {
           var IsValid= _authorService.check(authorViewModel.Name, authorViewModel.Id);
            return Json(IsValid);

        }
    }
}
