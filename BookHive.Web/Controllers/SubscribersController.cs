using BookHive.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookHive.Domain.Entities;

namespace BookHive.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IDataProtector _dataProtector;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;
        public SubscribersController(IApplicationDbContext context
            , IMapper mapper
            , IImageService imageService
            , IDataProtectionProvider dataProtector,
            IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = PopulateModel();
            return View("Form", viewModel);

        }

        public async Task<IActionResult> Renew(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers.Include(x => x.subscribtions).SingleOrDefault(x => x.Id == subscriberId);

            if (subscriber is null)
            {
                return NotFound();
            }

            if (subscriber.IsDeleted)
            {
                return BadRequest();
            }

            var lastEndDate = subscriber.subscribtions.Select(x => x.EndDate).Last();
            var StartDate = DateTime.Today > lastEndDate ? DateTime.Today : lastEndDate.AddDays(1);
            var EndDate = StartDate.AddYears(1);

            Subscribtion subscribtion = new Subscribtion()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedOn = DateTime.Now,
                StartDate = StartDate,
                EndDate = EndDate
            };
            subscriber.subscribtions.Add(subscribtion);
            _context.SaveChanges();
            var body = _emailBodyBuilder.GetEmailBody("https://th.bing.com/th/id/OIP.9oxlutL9_TtNvUIxctfT0wHaHa?rs=1&pid=ImgDetMain",
            $"Hey {subscriber.FirstName}, thanks for joining us!",
            "https://www.google.com/",
            "Active Account",
            "please Confirm your Subscribtion");

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "New Subscription", body));
            //BackgroundJob.Schedule(() => _emailSender.SendEmailAsync(subscriber.Email, "New Subscription", body),TimeSpan.FromMinutes(1));


            var model = _mapper.Map<SubscriptionViewModel>(subscribtion);
            return PartialView("_SubscriptionRow", model);
        }

        public IActionResult Details(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers
                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.subscribtions)
                .Include(s => s.Rentals) // filter rentals
                .ThenInclude(s => s.RentalCopy)

                .SingleOrDefault(s => s.Id == subscriberId)
                ;

            if (subscriber is null)
                return NotFound();
            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;

            return View(viewModel);
        }

        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var Areas = _context.Areas.Where(x => x.GovernorateId == governorateId && !x.IsDeleted).ToList();
            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(Areas));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateModel(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
            var imagePath = "/images/Subscribers";

            var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, imagePath, hasThumbnail: true);

            if (!isUploaded)
            {
                ModelState.AddModelError("Image", errorMessage!);
                return View("Form", PopulateModel(model));
            }
            subscriber.ImageUrl = $"{imagePath}/{imageName}";
            subscriber.ImageThumbnailUrl = $"{imagePath}/thumb/{imageName}";
            subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Subscribers.Add(subscriber);
            _context.SaveChanges();

            Subscribtion subscribtion = new Subscribtion()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedOn = DateTime.Now,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)
            };
            subscriber.subscribtions.Add(subscribtion);
            _context.SaveChanges();

            var body = _emailBodyBuilder.GetEmailBody("https://th.bing.com/th/id/OIP.9oxlutL9_TtNvUIxctfT0wHaHa?rs=1&pid=ImgDetMain",
                            $"Hey {subscriber.FirstName}, thanks for joining us!",
                            "https://www.google.com/",
                            "Active Account",
                            "please Confirm your Subscribtion");

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "New Subscription", body));



            //TODO: Send welcome email
            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());

            return RedirectToAction(nameof(Details), new { id = subscriberId });
        }



        [HttpGet]
        public IActionResult Edit(string id)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers.FirstOrDefault(x => x.Id == subscriberId);
            if (subscriber is null)
            {
                return NotFound();
            }
            var model = _mapper.Map<SubscriberFormViewModel>(subscriber);
            var viewModel = PopulateModel(model);
            viewModel.key = id;
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //Searching For Subscriber using (Email,NationalId,MobileNumber)
        public IActionResult Search(SearchFormViewModel search)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscribers
                            .SingleOrDefault(s =>
                                    s.Email == search.Value
                                || s.NationalId == search.Value
                                || s.MobileNumber == search.Value);


            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Form", PopulateModel(model));
            }
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.key!));
            var subscriber = _context.Subscribers.FirstOrDefault(x => x.Id == subscriberId);
            if (subscriber == null)
            {
                return NotFound();
            }
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(subscriber.ImageUrl))
                {
                    _imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);
                    //await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }
                var imageName = $"{Guid.NewGuid}{Path.GetExtension(model.Image.FileName)}";

                var result = await _imageService.UploadAsync(model.Image, imageName, "/images/subscribers/", true);

                if (!result.IsUploaded)
                {
                    ModelState.AddModelError(nameof(model.Image), result.errorMessage!);
                    return View("Form", PopulateModel(model));
                }
                model.ImageUrl = $"/images/{imageName}";
                model.ImageThumbnailUrl = $"/images/subscribers/Thumb/{imageName}";
            }
            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl;
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
            }

            subscriber = _mapper.Map(model, subscriber);
            subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            subscriber.LastUpdateOn = DateTime.Now;

            _context.SaveChanges();

            //var subId =_dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = model.key });


        }

        public IActionResult UniqueEmail(SubscriberFormViewModel mode)
        {
            int id = 0;
            if (!string.IsNullOrEmpty(mode.key))
                id = int.Parse(_dataProtector.Unprotect(mode.key));
            var subscriber = _context.Subscribers.SingleOrDefault(x => x.Email == mode.Email);

            var IsValid = subscriber is null || subscriber!.Id.Equals(id);
            return Json(IsValid);
        }

        public IActionResult UniqueNationalId(SubscriberFormViewModel mode)
        {
            int id = 0;
            if (!string.IsNullOrEmpty(mode.key))
                id = int.Parse(_dataProtector.Unprotect(mode.key));
            var subscriber = _context.Subscribers.SingleOrDefault(x => x.NationalId == mode.NationalId);

            var IsValid = subscriber is null || subscriber!.Id.Equals(id);

            return Json(IsValid);
        }

        public IActionResult UniqueMobileNumber(SubscriberFormViewModel mode)
        {
            int id = 0;
            if (!string.IsNullOrEmpty(mode.key))
                id = int.Parse(_dataProtector.Unprotect(mode.key));
            var subscriber = _context.Subscribers.SingleOrDefault(x => x.MobileNumber == mode.MobileNumber);

            var IsValid = subscriber is null || subscriber!.Id.Equals(id);
            return Json(IsValid);
        }

        public IActionResult SendExpirationAlerts()
        {
            RecurringJob.AddOrUpdate(() => PrepareExpirationAlert(), "0 14 * * *");
            return Ok();

        }

        public async Task PrepareExpirationAlert()
        {
            var subscribers = _context.Subscribers
                .Include(x => x.subscribtions)
                .Where(x => x.subscribtions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5))
                .ToList();


            foreach (var subscriber in subscribers)
            {
                var body = _emailBodyBuilder.GetEmailBody("https://th.bing.com/th/id/OIP.9oxlutL9_TtNvUIxctfT0wHaHa?rs=1&pid=ImgDetMain",
                           $"Hey {subscriber.FirstName}, Alert!!!",
                           "https://www.google.com/",
                           "Active Account",
                           $"Your subscribtion will be ended within 5 days {subscriber.subscribtions.Last().EndDate.ToString()}");

                await _emailSender.SendEmailAsync(subscriber.Email, "New Subscription", body);
            }
        }

        public SubscriberFormViewModel PopulateModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var Governorates = _context.Governorates.Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToList();



            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(Governorates);

            if (model?.GovernorateId > 0)
            {
                var Areas = _context.Areas.Where(x => x.GovernorateId == model.GovernorateId && !x.IsDeleted).OrderBy(x => x.Name).ToList();



                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(Areas);
            }

            return viewModel;
        }
    }
}
