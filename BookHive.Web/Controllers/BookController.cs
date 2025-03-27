using BookHive.Web.consts;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq.Dynamic.Core;

namespace BookHive.Web.Controllers
{
    public class BookController : Controller
    {

        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private List<string> _allowedExtensions = new() { ".jpg", ".png", ".jpeg" };
        private int _maxAllowedSize = 2097152;
        private readonly Cloudinary _cloudinary;
        public BookController(ApplicationDbContext context,IMapper mapper, IWebHostEnvironment environment,IOptions<CloudinarySettings> options)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            var account = new Account()
            {
                ApiKey=options.Value.APIKey,
                ApiSecret=options.Value.APISecrete,
                Cloud=options.Value.CloudName,
            };
            _cloudinary = new Cloudinary(account);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetBooks()
        {
            var skip =int.Parse(Request.Form["start"]);
            var take = int.Parse(Request.Form["length"]);
            var SortByColumnIndex= Request.Form["order[0][column]"];
            var sortByColumnName = Request.Form[$"columns[{SortByColumnIndex}][name]"];
            var sortColumnDirection = Request.Form["order[0][dir]"];
            var SearchBy = Request.Form["search[value]"];


            IQueryable<Book> books = _context.Books.
                Include(x=>x.Author).
                Include(x => x.Categories).
                ThenInclude(x => x.Category);

            if (!string.IsNullOrEmpty(SearchBy))
            {
                books = books.Where(x => x.Title.Contains(SearchBy)|| x.Author!.Name.Contains(SearchBy));
            }

            books = books.OrderBy($"{sortByColumnName} {sortColumnDirection}");
            var data = books.Skip(skip).Take(take).ToList();
            var recordsTotal = books.Count();
            var bookViewModel = _mapper.Map<IEnumerable<BookViewModel>>(data);
            var jsonData = new { recordsFiltered = recordsTotal, recordsTotal,data=bookViewModel };
            return Ok(jsonData);
        }


        public IActionResult Details(int id)
        {
            var book = _context.Books.
                Include(x=>x.Author).
                Include(x=>x.Copies).
                Include(x=>x.Categories).
                ThenInclude(x=>x.Category).
                FirstOrDefault(x => x.Id == id);
            if(book is  null)
            {
                return NotFound();
            }

            var bookViewModel=_mapper.Map<BookViewModel>(book); 
            return View(bookViewModel);
        }
        [HttpGet]
        public IActionResult Create()
        {
            
            return View("Form",PopulateViewModel());
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

            if(model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    //Validation depends on business rules
                    ModelState.AddModelError(nameof(model.Image), Validationscs.AllowedExtension);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length>_maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Validationscs.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }


                //The controller receives the image in the model.Image property(IFormFile).
                //The server constructs the destination path(path).
                //It creates/ overwrites a file at that location(File.Create(path)).
                //It copies the uploaded file contents to the new file(CopyToAsync(stream)).
                var ImageName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine($"{_environment.WebRootPath}/images/books", ImageName);
                var pathThumb = Path.Combine($"{_environment.WebRootPath}/images/books/Thumb", ImageName);
                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                stream.Dispose();
                book.ImageUrl = $"/images/books/{ImageName}";
                book.ImageUrlThumb = $"/images/books/Thumb/{ImageName}";

                //For saving thumb Image in thumb folder 
                using var image=Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height = image.Height / ratio;
                image.Mutate(i => i.Resize(width: 200, height: (int)height));
                image.Save(pathThumb);


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

            foreach(var category in model.SelectedCategories)
            {
                book.Categories.Add(new BookCategory()
                {
                    CategoryId=category, 
                    
                });
               
            }
            _context.Add(book);
            _context.SaveChanges();
           
            return RedirectToAction(nameof(Details),new {id=book.Id});
        }

        [HttpGet]
        //first I need to retrive book after getting book I need to map it to FormViewModel
        //FormViewModel model=PopulateViewModel(model); authorId--->authorId
        //SelectedCategories--->
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b=>b.Categories).SingleOrDefault(x=>x.Id==id);
            if (book == null) {

                return NotFound();
            }
            BookFormViewModel bookFormViewModel = _mapper.Map<BookFormViewModel>(book);
            bookFormViewModel.SelectedCategories = book.Categories.Select(x => x.CategoryId).ToList();
            
            return View("Form",PopulateViewModel(bookFormViewModel));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View("Form",PopulateViewModel(model));
            }
            var book = _context.Books.Include(x=>x.Categories).SingleOrDefault(x => x.Id == model.Id);
            if (book == null)
            {
                return NotFound();
            }
            string imagePublicId = null;
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    var oldImage =$"{_environment.WebRootPath}{book.ImageUrl}";
                    var oldImageThumb =$"{_environment.WebRootPath}{book.ImageUrlThumb}";
                    if (System.IO.File.Exists(oldImage))
                        System.IO.File.Delete(oldImage);

                    if (System.IO.File.Exists(oldImageThumb))
                        System.IO.File.Delete(oldImageThumb);
                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Validationscs.AllowedExtension);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Validationscs.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }

                //using var stream = model.Image.OpenReadStream();
                //var imageParams = new ImageUploadParams
                //{
                //    File = new FileDescription(ImageName, stream)
                //};
                //var result = await _cloudinary.UploadAsync(imageParams);

                //model.ImageUrl = result.SecureUrl.ToString();
                //imagePublicId = result.PublicId;
                var ImageName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine($"{_environment.WebRootPath}/images/books", ImageName);
                var pathThumb = Path.Combine($"{_environment.WebRootPath}/images/books/Thumb", ImageName);
                using var stream = System.IO.File.Create(path);
                await model.Image.CopyToAsync(stream);
                stream.Dispose();
                model.ImageUrl = $"/images/books/{ImageName}";
                model.ImageUrlThumb = $"/images/books/Thumb/{ImageName}";

                //For saving thumb Image in thumb folder 
                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var height = image.Height / ratio;
                image.Mutate(i => i.Resize(width: 200, height: (int)height));
                image.Save(pathThumb);
            }
            else if(!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl=book.ImageUrl;
                model.ImageUrlThumb = book.ImageUrlThumb;
            }
           
            book = _mapper.Map(model, book);
            //book.ImageUrlThumb = ConvertUrl(book.ImageUrl!);
            //book.ImagePublicId = imagePublicId;


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
            var Book = _context.Books.SingleOrDefault(x => x.Title == bookFormViewModel.Title && x.AuthorId== bookFormViewModel.AuthorId);

            var IsValid = Book is null || Book.Id.Equals(bookFormViewModel.Id);

            return Json(IsValid);

        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.SingleOrDefault(x => x.Id == id);
            if (book is  null)
            {
                return NotFound();
            }
            book.IsDeleted=!book.IsDeleted;
            book.LastUpdateOn=DateTime.Now;
            _context.SaveChanges();
            return Ok(book.LastUpdateOn.ToString());
        }

      

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model=null)
        {
            var authors = _context.Authors.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();
            var categories = _context.categories.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();
            
            

            if (model == null)
            {
                model = new BookFormViewModel()
                {
                    Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                    Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
                };
            }
            else
            {
                model.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
                model.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            }
            
            return model;
        }

        private string ConvertUrl(string url)
        {
            var separator = "image/upload/";
            var UrlParts=url.Split(separator);


            return $"{UrlParts[0]}{separator}c_thumb,w_200,g_face/{UrlParts[1]}";

        }
    }
}

//https://res.cloudinary.com/devdotnet/image/upload/v1741670612/fv3hbzkaets4wskhssyo.png       //OrginalImage
//https://res.cloudinary.com/devdotnet/image/upload/c_thumb,w_200,g_face/v1741670612/fv3hbzkaets4wskhssyo.png
