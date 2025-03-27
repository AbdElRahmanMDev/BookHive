

namespace BookHive.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly  ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            List<Category> categories= _context.categories.AsNoTracking().ToList();
            var categoryViewModels= _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(categoryViewModels);
        }

        [AjaxOnly]
        [HttpGet]
        public IActionResult Create()   //first you should know who invoke this method
        {
            return PartialView("_form");
        }

        [HttpPost]
        public IActionResult Create(CategoryFormViewModel categoryFormView)
        {
            if (!ModelState.IsValid) {
                return BadRequest();
            }
            var category = _mapper.Map<Category>(categoryFormView);
                _context.categories.Add(category);
                _context.SaveChanges();
            var categoryView = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", categoryView);
    
        }
        [AjaxOnly]
        [HttpGet]
        public IActionResult Edit(int id)
        {

           var category=_context.categories.FirstOrDefault(x=>x.Id==id);
            if (category == null) { 
            return NotFound();
            }
            var CategoryFormView=_mapper.Map<CategoryFormViewModel>(category);
            return PartialView("_form", CategoryFormView);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel categoryFormViewModel)
        {
           
            var category = _context.categories.FirstOrDefault(x => x.Id == categoryFormViewModel.Id);
            if (category == null)
            {
                return NotFound();
            }
            //category.Name=categoryFormViewModel.Name;
            category=_mapper.Map(categoryFormViewModel,category);
            category.LastUpdateOn=DateTime.Now;
            _context.SaveChanges();
            var categoryView = _mapper.Map<CategoryViewModel>(category);
            return PartialView("_CategoryRow", categoryView);

        }

        [HttpPost]
        public IActionResult ToggleState(int id) {
            var category= _context.categories.Find(id);
            if(category is null)
            {
                return NotFound();
            }
            category.IsDeleted=!category.IsDeleted;
            category.LastUpdateOn= DateTime.Now;
            _context.SaveChanges();
            return Ok(category.LastUpdateOn.ToString());
        
        }

        public IActionResult checkUnique(CategoryFormViewModel categoryFormView)
        {
            
            var category=_context.categories.SingleOrDefault(x=>x.Name==categoryFormView.Name);
            var Isvalid= category is null || category.Id.Equals(categoryFormView.Id);

            return Json(Isvalid);
            
        }
    }
}
