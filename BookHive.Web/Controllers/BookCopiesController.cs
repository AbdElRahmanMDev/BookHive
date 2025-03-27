using BookHive.Web.Core.Models;
using BookHive.Web.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Web.Controllers
{
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;   
        }

        [HttpGet]
        public IActionResult Create(int bookid)
        {
            var book = _context.Books.Find(bookid);
            if (book is null)
            {
                return NotFound();
            }
            var ShowRentalInput = book.IsAvailableForRental;
            var BookCopyFormViewModel = new BookCopyFormViewModel()
            {
                BookId=bookid,
                ShowRentalInput=ShowRentalInput,
            };


            return PartialView("_form", BookCopyFormViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel bookCopy)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var book = _context.Books.SingleOrDefault(x=>x.Id==bookCopy.BookId);
            if (book is null)
            {
                return NotFound();
            }
            var NewCopy = new BookCopy()
            {
                EditionNumber=bookCopy.EditionNumber,
                IsAvailableForRental=book.IsAvailableForRental? bookCopy.IsAvailableForRental : false,
            };
            book.Copies.Add(NewCopy);
        
            _context.SaveChanges();
            var bookviewModel = _mapper.Map<BookCopyViewModel>(NewCopy);


            return PartialView("_bookCopyRow", bookviewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id) {

            var bookCopy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);
            if (bookCopy is null) { 
                
                return NotFound();
            }

            var bookCopyForm=_mapper.Map<BookCopyFormViewModel>(bookCopy);

            bookCopyForm.IsAvailableForRental = bookCopy.Book!.IsAvailableForRental;

            return PartialView("_form",bookCopyForm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Edit(BookCopyFormViewModel model) {

            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && model.IsAvailableForRental;
            copy.LastUpdateOn = DateTime.Now;

            _context.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);

        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Find(id);
            if (copy is  null) {
                return NotFound();
            }

            copy.LastUpdateOn = DateTime.Now;
            copy.IsDeleted=!copy.IsDeleted;
            _context.SaveChanges(); 

            return Ok(copy.LastUpdateOn);
        }



    }
}
