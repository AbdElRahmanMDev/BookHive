using AspNetCoreGeneratedDocument;
using BookHive.Web.consts;
using BookHive.Web.Core.Enums;
using BookHive.Web.Core.Models;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;

namespace BookHive.Web.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;
        public RentalsController(ApplicationDbContext context, IMapper mapper,
           IDataProtectionProvider dataProtector)
        {
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
        }
        public IActionResult Create(string skey)
        {
            var SubscriberId = int.Parse(_dataProtector.Unprotect(skey));
            var subscriber = _context.Subscribers
                .Include(x => x.subscribtions)
                .Include(x => x.Rentals)
                .ThenInclude(x => x.RentalCopy)
                .SingleOrDefault(x => x.Id == SubscriberId);
            if (subscriber is null)
            {
                return NotFound();
            }
            var result = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(result.ErrosMessage))
            {
                return View("NotAllowedRental", result.ErrosMessage);
            }

            var viewmodel = new RentalFormViewModel()
            {
                SubscriberKey = skey,
                MaxAllowedCopies = result.maxAllowedCopies,

            };
            return View("Form", viewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Searching For Copies (using SerialNumber)
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var copy = _context.BookCopies
                .Include(x => x.Book).
                SingleOrDefault(x => x.SerialNumber.ToString() == model.Value && !x.IsDeleted && !x.Book!.IsDeleted);
            if (copy is null)
            {
                return NotFound(Validationscs.InvalidSerialNumber);
            }

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
            {
                return BadRequest(Validationscs.NotAvaialableForRental);
            }
            //Check if copy is In Rental
            var IsCopyInrental = _context.RentalCopies.Any(x => x.BookCopyId == copy.Id && !x.ReturnDate.HasValue );
            if (IsCopyInrental) {                          //true  true returnDate ==null  false 
                                                            
                return BadRequest(Validationscs.CopyInRental);
            }

            var viewmodel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewmodel);
            //Check that copy is not in rental
        }


        [HttpPost]
        public IActionResult Create(RentalFormViewModel model) {

            if (!ModelState.IsValid) {
                return View("Form",model);
            }
            var SubscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = _context.Subscribers
                .Include(x => x.subscribtions)
                .Include(x => x.Rentals)
                .ThenInclude(x => x.RentalCopy)
                .SingleOrDefault(x => x.Id == SubscriberId);
            if (subscriber is null)
            {
                return NotFound();
            }
            var result = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(result.ErrosMessage))
            {
                return View("NotAllowedRental", result.ErrosMessage);
            }

            var (resultError, copies) = ValidateCopies(SubscriberId, model.SelectedCopies);
            if (!string.IsNullOrEmpty(resultError))
            {
                return View("NotAllowedRental", resultError);
            }

            Rental rental = new()
            {
                RentalCopy = copies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details),new {id=rental.Id});    
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var rental = _context.Rentals.Include(r => r.RentalCopy).SingleOrDefault(x => x.Id == id);
            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdateOn = DateTime.Now;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();

            var count = _context.RentalCopies.Count(x=>x.RentalId==id);

            return Ok(count);
        }

    
        public IActionResult Details(int id)
        {
            var Rental = _context.Rentals
                .Include(x => x.RentalCopy)
                .ThenInclude(x => x.bookCopy)
                .ThenInclude(x => x!.Book)
                .SingleOrDefault(x => x.Id == id);

            var model = _mapper.Map<RentalViewModel>(Rental);

            return View(model);

        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var rental = _context.Rentals
                .Include(x => x.RentalCopy)
                .ThenInclude(x => x.bookCopy)
                .SingleOrDefault(x => x.Id == id);
            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
                .Include(x => x.subscribtions)
                .Include(x => x.Rentals)
                .ThenInclude(x => x.RentalCopy)
                .SingleOrDefault(x => x.Id == rental.SubscriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);



            var currentCopiesIds = rental.RentalCopy.Select(c => c.BookCopyId).ToList();

            var currentCopies = _context.BookCopies
                .Where(c => currentCopiesIds.Contains(c.Id))
                .Include(c => c.Book)
                .ToList();

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAllowedCopies = maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }
            var rental = _context.Rentals
                .Include(x => x.RentalCopy)
                .ThenInclude(x => x.bookCopy)
                .SingleOrDefault(x => x.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));
            var subscriber = _context.Subscribers.
                Include(s => s.subscribtions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopy)
                .SingleOrDefault(s => s.Id == subscriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, model.Id);
            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (RentalsError, copies) = ValidateCopies(subscriberId,model.SelectedCopies,rental.Id);

            if (!string.IsNullOrEmpty(RentalsError))
                return View("NotAllowedRental", RentalsError);

            rental.RentalCopy = copies;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            rental.LastUpdateOn = DateTime.Today;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });

        }


        public IActionResult Return(int id)
        {
            var rental = _context.Rentals
               .Include(x => x.RentalCopy)
               .ThenInclude(x => x.bookCopy)
               .ThenInclude(x=>x!.Book)
               .SingleOrDefault(x => x.Id == id);
            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _context.Subscribers
                .Include(x => x.subscribtions)
                .Include(x => x.Rentals)
                .ThenInclude(x => x.RentalCopy)
                .SingleOrDefault(x => x.Id == rental.SubscriberId);


            var model = new ReturnFormViewModel()
            {
                Id = rental.Id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopy.Where(x=>!x.ReturnDate.HasValue)).ToList(),
                SelectedCopies = rental.RentalCopy.Where(x => !x.ReturnDate.HasValue).Select(x => new ReturnCopyViewModel() { Id = x.BookCopyId,IsReturned= x.ExtendedOn.HasValue ?false  : null }).ToList(),
                AllowExtend=!subscriber!.IsBlackListed 
                && subscriber.subscribtions.Last().EndDate >= rental.StartDate.AddDays((int)RentalConfiguration.MaxRentalDuration)
                && rental.StartDate.AddDays((int)RentalConfiguration.RentalDuration) >= DateTime.Today
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Return(ReturnFormViewModel model)
        {
            var rental = _context.Rentals
              .Include(x => x.RentalCopy)
              .ThenInclude(x => x.bookCopy)
              .ThenInclude(x => x!.Book)
              .SingleOrDefault(x => x.Id == model.Id);
            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopy.Where(x => !x.ReturnDate.HasValue).ToList());
                return View(model);
            }
            var subscriber = _context.Subscribers
                .Include(x => x.subscribtions)
                .Include(x => x.Rentals)
                .ThenInclude(x => x.RentalCopy)
                .SingleOrDefault(x => x.Id == rental.SubscriberId);
        
                
            if(model.SelectedCopies.Any(c=>c.IsReturned.HasValue && !c.IsReturned.Value))
            {
                string error=string.Empty;
                if(subscriber!.IsBlackListed)
                   error=Validationscs.RentalNotAllowedForBlackListed;
   
                else if (subscriber.subscribtions.Last().EndDate < rental.StartDate.AddDays((int)RentalConfiguration.MaxRentalDuration))
                    error = Validationscs.RentalNotAllowedForInActive;
                
                else if (rental.StartDate.AddDays((int)RentalConfiguration.RentalDuration) < DateTime.Today)
                   error= Validationscs.ExtendNotAllowed;

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopy.Where(x => !x.ReturnDate.HasValue)).ToList();
                    ModelState.AddModelError("", error);
                    return View(model);

                }

            }



            var isUpdated = false;
            foreach(var copy in model.SelectedCopies)
            {
                if(!copy.IsReturned.HasValue ) continue;

                var currentcopy=rental.RentalCopy.SingleOrDefault(x=>x.bookCopy!.Id==copy.Id);

                if (currentcopy is null) continue;

                if(copy.IsReturned.HasValue && copy.IsReturned.Value)
                {
                    if (currentcopy.ReturnDate.HasValue)
                        continue;
                    currentcopy.ReturnDate = DateTime.Now;
                    isUpdated = true;
                }
                if (copy.IsReturned.HasValue && !copy.IsReturned.Value)
                {
                    if (currentcopy.ExtendedOn.HasValue)
                        continue;
                    currentcopy.ExtendedOn = DateTime.Now;
                    currentcopy.EndDate=currentcopy.RentalDate.AddDays((int)RentalConfiguration.MaxRentalDuration);
                    isUpdated = true;
                }

            }

            if (isUpdated)
            {
                rental.LastUpdateOn=DateTime.Now;
                rental.LastUpdatedById= User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                rental.PenaltyPaid = model.PenaltyPaid;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });


        }
        private (string ErrosMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber, int? rentalId= null)
        {

            if (subscriber.IsBlackListed == true)
                return (Validationscs.IsBlackListed, null);


            if (subscriber.subscribtions.Last().EndDate < DateTime.Today.AddDays((int)RentalConfiguration.RentalDuration))
                return (Validationscs.InActive, null);

            var currentRentals = subscriber.Rentals
                .Where(r=>rentalId==null || r.Id!=rentalId)
                .SelectMany(x => x.RentalCopy).Count(x => !x.ReturnDate.HasValue);

            var avaiableCopiesCount = (int)RentalConfiguration.MaxAllowedCopies - currentRentals;

            if (avaiableCopiesCount.Equals(0))
                return (Validationscs.MaxAllowed, null);

            return (string.Empty, avaiableCopiesCount);

        }
       
        private (string ErrorMessage, ICollection<RentalCopy> copies) ValidateCopies(int SubscriberId, IEnumerable<int> selectedCopies, int? RentalId=null)
        {
            var subscriber = _context.Subscribers.Include(x => x.subscribtions).Include(x => x.Rentals).ThenInclude(x => x.RentalCopy)
                .SingleOrDefault(x => x.Id == SubscriberId);
            var SelectedCopies = _context.BookCopies
                .Include(x => x.Book)
                .Include(x => x.Rentals)
                .Where(x => selectedCopies
                .Contains(x.SerialNumber));

            //You Need book Ids for 
            var currentsubscriberRentals = _context.Rentals
                .Include(x => x.RentalCopy)
                .ThenInclude(x => x.bookCopy)
                .Where(x => x.SubscriberId == SubscriberId &&(RentalId ==null || x.Id != RentalId))
                .SelectMany(x => x.RentalCopy)
                .Where(x => !x.ReturnDate.HasValue)
                .Select(x => x.bookCopy!.BookId)
                .ToList();
            List<RentalCopy> copies = new List<RentalCopy>();

            foreach (var copy in SelectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (Validationscs.NotAvaialableForRental, copies);
                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && (RentalId==null ||  c.RentalId != RentalId)))
                {
                    return (Validationscs.CopyInRental, copies);
                }

                if (currentsubscriberRentals.Any(bookId => bookId == copy.BookId))

                    return ($"This Subscriber already has a copy for  '{copy.Book.Title}'", copies);
                copies.Add(new RentalCopy()
                {
                    BookCopyId = copy.Id
                });

            }
            return (string.Empty, copies);

        }
    }
}
