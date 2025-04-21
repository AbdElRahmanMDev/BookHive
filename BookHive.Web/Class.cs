using AspNetCoreGeneratedDocument;
using BookHive.Web.consts;
using BookHive.Web.Core.Enums;
using BookHive.Web.Core.Models;
using BookHive.Web.Services;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Immutable;
using System.Linq.Dynamic.Core;

namespace BookHive.Web
{
    public class RentalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtector _dataProtector;

        public RentalController(ApplicationDbContext context,
            IMapper mapper,
            IImageService service,
            IEmailBodyBuilder email,
            IEmailSender emailSender,
            IDataProtector dataProtector
            )
        {

            _context = context;
            _mapper = mapper;
            _imageService = service;
            _emailBodyBuilder = email;
            _emailSender = emailSender;
            _dataProtector = dataProtector;

        }
        

        public IActionResult Create(string sKey)
        {
            var subscriberId=int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber=_context.Subscribers
                .Include(x => x.subscribtions)
                .Include(x=>x.Rentals)
                .ThenInclude(x=>x.RentalCopy)
                .SingleOrDefault(x=>x.Id == subscriberId);

            if (subscriber is null || subscriber.IsDeleted)
                return NotFound();

            if (subscriber.IsBlackListed)
                return View("NotAllowedRental", Validationscs.IsBlackListed);

            var lastSubscribtion = subscriber.subscribtions.Last();

            if (lastSubscribtion.EndDate < DateTime.Today.AddDays(7))
                return View("NotAllowedRental", Validationscs.NotAvaialableForRental);

            var currentCopies=subscriber.Rentals.SelectMany(x=>x.RentalCopy).Count(x=>!x.ReturnDate.HasValue);
            var availableCopies=(int)(RentalConfiguration.MaxAllowedCopies)-currentCopies;
            if (availableCopies > 0)
                return View("NotAllowedRental", Validationscs.CopyInRental);
            RentalFormViewModel viewModel = new RentalFormViewModel()
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = availableCopies,
            };

            return View("Form", viewModel);
        }

        public IActionResult Cancel(int id)
        {
            var rental=_context.Rentals
                .Include(x=>x.RentalCopy)
                .ThenInclude(x=>x.bookCopy)
                .SingleOrDefault(x=>x.Id == id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            rental.LastUpdateOn = DateTime.Today;

            var currentCopies = rental.RentalCopy.Count();

            _context.SaveChanges();
            return Ok(currentCopies);
        }
        public IActionResult GetCopy(SearchFormViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies
                .Include(x => x.Book)
                .Include(x => x.Rentals)
                .SingleOrDefault(x => x.SerialNumber.ToString() == model.Value&&!x.IsDeleted && !x.Book!.IsDeleted);
            if(copy is null)
                return NotFound(Validationscs.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Validationscs.NotAvaialableForRental);

            var IsCopyInRental=_context.RentalCopies.Any(x=>!x.ReturnDate.HasValue && x.BookCopyId==copy.Id);         
            if( copy.Rentals.Any(r => !r.ReturnDate.HasValue))
                return BadRequest(Validationscs.CopyInRental);

           var copyview=_mapper.Map<BookCopyViewModel>(copy);
            return PartialView("",copyview);
        }


        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = _context.Subscribers.
                Include(x => x.subscribtions)
                .Include(x => x.Rentals)
                .ThenInclude(x=>x.RentalCopy)
                .SingleOrDefault(x => x.Id == subscriberId);

            if (subscriber is null || subscriber.IsDeleted)
                return NotFound();

            (string ErrorMessage, int? count) = Fun(subscriber);
            if (string.IsNullOrEmpty(ErrorMessage))
                return View("NotAllowedRental", ErrorMessage);

            var SelectedCopies = _context.BookCopies
                .Include(x => x.Book)
                .Include(x => x.Rentals)
                .Where(x => model.SelectedCopies.Contains(x.SerialNumber)).ToList();


            var bookIdss = _context.Rentals.
                Include(x => x.RentalCopy)
                .ThenInclude(x => x.bookCopy)
                .Where(x => x.SubscriberId == subscriberId)
                .SelectMany(x => x.RentalCopy)
                .Where(x => !x.ReturnDate.HasValue)
                .Select(x => x.bookCopy!.BookId)
                .ToList();
            
            var RentalCopy = new List<RentalCopy>();

            foreach (var copy in SelectedCopies)
            {
                if (copy.IsAvailableForRental || copy.Book!.IsAvailableForRental)
                    return View("NotAllowedRental", Validationscs.NotAvaialableForRental);

                if (copy.Rentals.Any(r => !r.ReturnDate.HasValue))
                    return View("NotAllowedRental", Validationscs.CopyInRental);
                if (bookIdss.Contains(copy.BookId))
                    return View("NotAllowedRental", $"This subscriber already has a copy for '{copy.Book.Title}' Book");
                RentalCopy.Add(new RentalCopy
                {
                    bookCopy=copy  
                });
            }

            var rental = new Rental()
            {
                RentalCopy = RentalCopy,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental); 
            _context.SaveChanges();

            return RedirectToAction(nameof(Index),new {id=rental.Id});
        }


        public IActionResult Details(int id)
        {
            var rental=_context.Rentals
                .Include(x=>x.RentalCopy)
                .ThenInclude(x=>x.bookCopy)
                .SingleOrDefault(x=>x.Id==id);

            if (rental is null)
                return NotFound();

            var model = _mapper.Map<RentalViewModel>(rental);

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var rental=_context.Rentals.
              
                Include(x=>x.RentalCopy)
                .ThenInclude(x=>x.bookCopy)
                .SingleOrDefault(x=>x.Id == id);
            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber=_context.Subscribers
                        .Include(x=>x.subscribtions)
                        .Include(x=>x.Rentals)
                        .ThenInclude(x=>x.RentalCopy)
                        .SingleOrDefault(x=>x.Id==rental.SubscriberId);

            if (subscriber is null)
                return NotFound();

            (string ErrorMessage, int? count) = Fun(subscriber);

            if (string.IsNullOrEmpty(ErrorMessage))
                return View("NotAllowedForRentals", ErrorMessage);


            var rentalCopy = rental.RentalCopy.Select(x => x.BookCopyId).ToList();
            var _CopiesInRental=_context.BookCopies.Where(x=>rentalCopy.Contains(x.Id)).ToList();



            RentalFormViewModel model = new RentalFormViewModel()
            {
               
                SubscriberKey = _dataProtector.Protect(subscriber.Id.ToString()),
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(_CopiesInRental),
                MaxAllowedCopies=count
            };

            return View("Form",model);
        }

        private (string ErrorMessage,int? count) Fun(Subscriber subscriber)
        {
        

            if (subscriber.IsBlackListed)
                return (Validationscs.IsBlackListed,null);

            var lastSubscribtion = subscriber.subscribtions.Last();

            if (lastSubscribtion.EndDate < DateTime.Today.AddDays(7))
                return (Validationscs.NotAvaialableForRental,null);

            var currentCopies = subscriber.Rentals.SelectMany(x => x.RentalCopy).Count(x => !x.ReturnDate.HasValue);
            var availableCopies = (int)(RentalConfiguration.MaxAllowedCopies) - currentCopies;
            if (availableCopies.Equals(0))
                return (Validationscs.MaxAllowed,null);

            return (string.Empty,availableCopies);


        }
    }
}
