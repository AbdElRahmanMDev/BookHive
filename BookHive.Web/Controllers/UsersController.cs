using BookHive.Web.consts;
using BookHive.Web.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookHive.Web.Controllers
{
    [Authorize(Roles =AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
        }


        public async Task<IActionResult> Index()
        {
            var users=await _userManager.Users.ToListAsync();

            var ViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(ViewModel);
        }

        [HttpGet]
        public async  Task<IActionResult> Create()
        {
     
            UserFormViewModel userFormView = new UserFormViewModel()
            {
                Roles= await _roleManager.Roles.Select(r=>new SelectListItem
                {
                    Text= r.Name,
                    Value=r.Name,
                }).ToListAsync()
            };
            return PartialView("_form",userFormView);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid) {
                return BadRequest();    
            }

            ApplicationUser user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

           var result= await _userManager.CreateAsync(user,model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user,model.SelectedRoles);
                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',',result.Errors.Select(x=>x.Description)));
        }



        public async Task<IActionResult> checkEmail(UserFormViewModel userFormViewModel)
        {
            var user = await _userManager.FindByEmailAsync(userFormViewModel.Email);
            var Isvalid=user is null || user.Id.Equals(userFormViewModel.Id);
            return Json(Isvalid);
        }


            
        public async Task<IActionResult> checkUserName(UserFormViewModel userFormViewModel)
        {
            var user = await _userManager.FindByNameAsync(userFormViewModel.UserName);
            var Isvalid = user is null || user.Id.Equals(userFormViewModel.Id);

            return Json(Isvalid);
        }

        
        public async Task<IActionResult> ToggleState(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user is null)
            {
                return NotFound();
            }
            user.IsDeleted = !user.IsDeleted;
            user.LastUpdateOn=DateTime.Now;
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var model = _mapper.Map<UserViewModel>(user);
                return Ok(model.LastUpdateOn.ToString());
            }
            return BadRequest(string.Join(',', result.Errors.Select(x => x.Description)));
        }

        [HttpGet]

        public async Task<IActionResult> ChangePassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                return NotFound();
            }
            var model = new PasswordFormViewModel()
            {
                Id = user.Id
            };
            return PartialView("_ChangePassword",model);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(PasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var currentPasswordHash = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, model.Password);

            if (result.Succeeded)
            {
                user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdateOn = DateTime.Now;

                await _userManager.UpdateAsync(user);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            user.PasswordHash = currentPasswordHash;
            await _userManager.UpdateAsync(user);

            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user is null)
            {
                return NotFound();
            }

            var model=_mapper.Map<UserFormViewModel>(user);

            model.SelectedRoles=await _userManager.GetRolesAsync(user);
            model.Roles= await _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name,
            }).ToListAsync();

            return PartialView("_form", model);
        }

        [HttpPost]

        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid) {
                ViewBag.Erros = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest();
            }
            var user = await _userManager.FindByIdAsync(model.Id);
            if(user is null)
            {
                return NotFound();
            }
            user = _mapper.Map(model, user);
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdateOn = DateTime.Now;

           var result=await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roles);
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                var modelView = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", modelView);
            }
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        //AQAAAAEAACcQAAAAEKvfVcTNpAb+mGNhS2sa7tUkjq91HrtSuLXYaI6nPtgytFMoMNsUfnPiIsEkChFSbg==
        //AQAAAAEAACcQAAAAEOJue/9i1fmiGQ5J1gRziwYdjxWmD0051Upkl3F541RlLHW24wKM6y+YPkJRh4HteA==
    }
}
