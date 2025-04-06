using BookHive.Web.consts;
using BookHive.Web.Core.Models;
using BookHive.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace BookHive.Web.Controllers
{
    [Authorize(Roles =AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;


        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment,IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
        }


        public async Task<IActionResult> Index()
        {
            //var filepath = $"{_webHostEnvironment.WebRootPath}/Templates/email.html";
            //StreamReader sr = new StreamReader(filepath);
            //var body = sr.ReadToEnd();
            //sr.Close();
            //body = body
            //         .Replace("[imageUrl]", "https://res.cloudinary.com/devcreed/image/upload/v1668732314/icon-positive-vote-1_rdexez.svg")
            //         .Replace("[header]", "Hey Abdo , Thanks for joining us!")
            //         .Replace("[url]", "https://www.google.com")
            //         .Replace("[linkTitle]", "Active Account")
            //         .Replace("[body]", "please Confirm your email");

            //await _emailSender.SendEmailAsync(email: "abdelrahman.m.elsayedd@gmail.com", "Test Email", body);
            var users=await _userManager.Users.ToListAsync();

            var ViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(ViewModel);
        }

        [HttpGet]
        public async  Task<IActionResult> Create()
        {

            UserFormViewModel userFormView = new UserFormViewModel()
            {
                Roles = await _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name,
                }).ToListAsync()
            };
            return PartialView("_form", userFormView);
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
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code },
                    protocol: Request.Scheme);


              var body = _emailBodyBuilder.GetEmailBody("https://res.cloudinary.com/devcreed/image/upload/v1668732314/icon-positive-vote-1_rdexez.svg",
              $"Hey {user.FullName}, thanks for joining us!",
              $"{HtmlEncoder.Default.Encode(callbackUrl!)}",
              "Active Account",
              "please Confirm your email");
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);

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


        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdateOn = DateTime.Now;

            await _userManager.UpdateAsync(user);

            if (user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);
            if (user.IsDeleted)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            return Ok(user.LastUpdateOn.ToString());
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
                await _userManager.UpdateSecurityStampAsync(user);
                var modelView = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", modelView);
            }
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }


        public async Task<IActionResult> Unlock(string id)
        {
            var user=await _userManager.FindByIdAsync(id);
            if (user is null) { 
                return NotFound();
            }
            var islocked=await _userManager.IsLockedOutAsync(user);
            if (islocked) {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }

            return Ok();
        }

    }
}
