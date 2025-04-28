using Microsoft.Identity.Client;
using System.Linq.Expressions;

namespace BookHive.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archieve)]
    public class BookCopiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public BookCopiesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork=unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Create(int bookid)
        {
            var book = _unitOfWork.Books.GetById(bookid);
            if (book is null)
            {
                return NotFound();
            }
            var ShowRentalInput = book.IsAvailableForRental;
            var BookCopyFormViewModel = new BookCopyFormViewModel()
            {
                BookId = bookid,
                ShowRentalInput = ShowRentalInput,
            };


            return PartialView("_form", BookCopyFormViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel bookCopy)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var book = _unitOfWork.Books.GetById(bookCopy.BookId);
            if (book is null)
            {
                return NotFound();
            }
            var NewCopy = new BookCopy()
            {
                EditionNumber = bookCopy.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental ? bookCopy.IsAvailableForRental : false,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };
            //you add new copy through navigation Property in book model
            book.Copies.Add(NewCopy);

            _unitOfWork.Complete();
            var bookviewModel = _mapper.Map<BookCopyViewModel>(NewCopy);


            return PartialView("_bookCopyRow", bookviewModel);
        }

      

        public IActionResult Rentals(int id)
        {
            var RentalsCopy = _unitOfWork.RentalCopies
                .FindAll(x => x.BookCopyId == id,
                x => x.
                Include(x => x.Rentals)!.
                ThenInclude(x => x!.Subscriber!),
                orderBy:c=>c.RentalDate,
                orderByDirection: OrderBy.Descending);
            
                //_context.RentalCopies
                //.Include(x => x.Rentals)
                //.ThenInclude(x => x!.Subscriber)
                //.Include(x => x.bookCopy)
                //.Where(x => x.BookCopyId == id)
                //.OrderByDescending(x => x.RentalDate)
                //.ToList();



            var model = _mapper.Map<IEnumerable<RentalHistoryViewModel>>(RentalsCopy);

            return View(model);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
       
            var bookCopy = _unitOfWork.BookCopies.Find(c => c.Id == id,c=>c.Include(x=>x.Book)!);
                //_context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);
            if (bookCopy is null)
            {

                return NotFound();
            }

            var bookCopyForm = _mapper.Map<BookCopyFormViewModel>(bookCopy);

            bookCopyForm.IsAvailableForRental = bookCopy.Book!.IsAvailableForRental;

            return PartialView("_form", bookCopyForm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Edit(BookCopyFormViewModel model)
        {

            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _unitOfWork.BookCopies.Find(x => x.Id == model.Id, x => x.Include(x => x.Book)!);
                //_context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && model.IsAvailableForRental;
            copy.LastUpdateOn = DateTime.Now;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            //_context.SaveChanges();
            _unitOfWork.Complete();
            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);

        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _unitOfWork.BookCopies.GetById(id);
            if (copy is null)
            {
                return NotFound();
            }

            copy.LastUpdateOn = DateTime.Now;
            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _unitOfWork.Complete();

            return Ok(copy.LastUpdateOn);
        }



    }
}
