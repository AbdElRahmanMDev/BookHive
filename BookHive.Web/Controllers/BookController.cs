using BookHive.Application.Services.Books;
using BookHive.Domain.DTOS;
using BookHive.Web.Extensions;
using BookHive.Web.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;


namespace BookHive.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archieve)]
    public class BookController : Controller
    {

        private readonly IWebHostEnvironment _environment;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private List<string> _allowedExtensions = new() { ".jpg", ".png", ".jpeg" };
        private int _maxAllowedSize = 2097152;
        private readonly Cloudinary _cloudinary;
        private readonly IImageService _imageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IbookService _bookService;
        public BookController(IApplicationDbContext context
            , IMapper mapper,
            IWebHostEnvironment environment
            , IOptions<CloudinarySettings> options,
            IImageService imageService,
            IbookService bookService,
           IUnitOfWork unitOfWork
            )
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            _imageService = imageService;
            _unitOfWork = unitOfWork;
            _bookService = bookService;
            var account = new Account()
            {
                ApiKey = options.Value.APIKey,
                ApiSecret = options.Value.APISecrete,
                Cloud = options.Value.CloudName,
            };
            _cloudinary = new Cloudinary(account);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetBooks()
        {
            var dto = Request.Form.GetFilters();
           

            var (books, recordsTotal) = _bookService.GetFiltered(dto);
       
            var bookViewModel = _mapper.ProjectTo<BookViewModel>(books).ToList();
            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal, data = bookViewModel };
            return Ok(jsonData);
        }


        public IActionResult Details(int id)
        {
            var query = _unitOfWork.Books.GetDetails();
                
                //_unitOfWork.Books.GetQueryable().
                //Include(x => x.Author).
                //Include(x => x.Copies).
                //ThenInclude(x => x.Rentals).
                //Include(x => x.Categories).
                //ThenInclude(x => x.Category);
            var bookViewModel = _mapper.ProjectTo<BookViewModel>(query).FirstOrDefault(x => x.Id == id);
            

            if (bookViewModel is null)
            {
                return NotFound();
            }

            return View(bookViewModel);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Erros = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return View("Form", PopulateViewModel(model));
            }
            var book = _mapper.Map<Book>(model);

            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid}{Path.GetExtension(model.Image.FileName)}";

                var result = await _imageService.UploadAsync(model.Image, imageName, "/images/books/", true);

                if (!result.IsUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                book.ImageUrl = $"/images/books/{imageName}";
                book.ImageUrlThumb = $"/images/books/thumb/{imageName}";
                //using var stream = model.Image.OpenReadStream();
                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(ImageName, stream)
                //};
                //var result= await _cloudinary.UploadAsync(imageParams);

                //book.ImageUrl = result.SecureUrl.ToString();
                //book.ImageUrlThumb = ConvertUrl(book.ImageUrl);
                //book.ImagePublicId = result.PublicId;
            }
            book.CreatedById = User.getUserId();
            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory()
                {
                    CategoryId = category,

                });

            }
            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.Categories).SingleOrDefault(x => x.Id == id);
            if (book == null)
            {

                return NotFound();
            }
            BookFormViewModel bookFormViewModel = _mapper.Map<BookFormViewModel>(book);
            bookFormViewModel.SelectedCategories = book.Categories.Select(x => x.CategoryId).ToList();

            return View("Form", PopulateViewModel(bookFormViewModel));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Form", PopulateViewModel(model));
            }
            var book = _context.Books.Include(x => x.Categories).SingleOrDefault(x => x.Id == model.Id);
            if (book == null)
            {
                return NotFound();
            }
            string imagePublicId = null;
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    _imageService.Delete(book.ImageUrl, book.ImageUrlThumb);
                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }
                var imageName = $"{Guid.NewGuid}{Path.GetExtension(model.Image.FileName)}";

                var result = await _imageService.UploadAsync(model.Image, imageName, "/images/books/", true);

                if (!result.IsUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
                model.ImageUrl = $"/images/books/{imageName}";
                model.ImageUrlThumb = $"/images/books/Thumb/{imageName}";

                //using var stream = model.Image.OpenReadStream();
                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(ImageName, stream)
                //};
                //var result = await _cloudinary.UploadAsync(imageParams);

                //model.ImageUrl = result.SecureUrl.ToString();
                //imagePublicId = result.PublicId;
            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageUrlThumb = book.ImageUrlThumb;
            }

            book = _mapper.Map(model, book);
            //book.ImageUrlThumb = ConvertUrl(book.ImageUrl!);
            //book.ImagePublicId = imagePublicId;
            book.LastUpdatedById = User.getUserId();

            foreach (var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory()
                {
                    CategoryId = category,

                });

            }
            _context.SaveChanges();

            return RedirectToAction("Index");


        }
        public IActionResult check(BookFormViewModel bookFormViewModel)
        {
            var Book = _context.Books.SingleOrDefault(x => x.Title == bookFormViewModel.Title && x.AuthorId == bookFormViewModel.AuthorId);

            var IsValid = Book is null || Book.Id.Equals(bookFormViewModel.Id);

            return Json(IsValid);

        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.SingleOrDefault(x => x.Id == id);
            if (book is null)
            {
                return NotFound();
            }
            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = User.getUserId();
            book.LastUpdateOn = DateTime.Now;
            _context.SaveChanges();
            return Ok(book.LastUpdateOn.ToString());
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel book = model is null ? new BookFormViewModel() : model;

            var authors = _context.Authors.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();
            var categories = _context.categories.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();

            book.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            book.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return book;
        }

        private string ConvertUrl(string url)
        {
            var separator = "image/upload/";
            var UrlParts = url.Split(separator);


            return $"{UrlParts[0]}{separator}c_thumb,w_200,g_face/{UrlParts[1]}";

        }
    }
}

//https://res.cloudinary.com/devdotnet/image/upload/v1741670612/fv3hbzkaets4wskhssyo.png       //OrginalImage
//https://res.cloudinary.com/devdotnet/image/upload/c_thumb,w_200,g_face/v1741670612/fv3hbzkaets4wskhssyo.png
