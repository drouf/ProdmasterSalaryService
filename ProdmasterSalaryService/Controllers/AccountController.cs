using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProdmasterSalaryService.Models.Classes;
using ProdmasterSalaryService.Services.Interfaces;
using ProdmasterSalaryService.ViewModels.Account;

namespace ProdmasterSalaryService.Controllers
{
    [Route("account")]
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICustomService _customService;
        public AccountController(ICustomService customService, IUserService userService)
        {
            _userService = userService;
            _customService = customService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetUser();
            var model = await _userService.GetModelFromUser(user);
            return View(model);
        }
        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> Login([FromQuery(Name = "ReturnUrl")] string? returnUrl)
        {
            if (await Authorized()) return RedirectToAction(nameof(Index), "Account");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, [FromQuery(Name = "ReturnUrl")] string? returnUrl)
        {
            if (await Authorized()) return RedirectToAction(nameof(Index), "Account");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.GetByLogin(model.Login);

                    if (user != null)
                    {
                        if (user.Password == model.Password)
                        {
                            await Authenticate(model.Login);
                            //var returnUrl = HttpContext.Request.Query["ReturnUrl"].ToString();
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                return Redirect(returnUrl);
                            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
                        }
                        ModelState.AddModelError(nameof(LoginModel.Password), "Неправильный пароль");
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(LoginModel.Login), "Такой пользователь не зарегистрирован!");
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "Не удалось войти");
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet("register")]
        public async Task<IActionResult> Register()
        {
            if (await Authorized()) return RedirectToAction(nameof(Index), "Account");

            var custom = await _customService.GetCustoms();
            ViewBag.Custom = custom.ToList();

            return View(new RegisterModel());
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (await Authorized()) return RedirectToAction(nameof(Index), "Account");
            
            model.Name = (await _customService.GetByDisanId(disanId: model.DisanId)).Name;
            if (ModelState.IsValid)
            {
                try
                {
                    if (!await _userService.UserExists(model))
                    {
                        
                        var custom = await _customService.GetByDisanId(disanId: model.DisanId);
                        var user = await _userService.Add(model, custom);
                        if (user != null)
                        {
                            //user.Custom.User = user;
                            await Authenticate(user.Login);
                            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
                        }
                        else
                        {
                            ModelState.AddModelError("", "Не удалось сохранить пользователя!");
                        }
                        
                    }
                    else
                    {
                        ModelState.AddModelError("", "Пользователь с таким логином уже зарегистрирован!");
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "Не удалось войти");
                }
            }
            var customs = await _customService.GetCustoms();
            ViewBag.Custom = customs.ToList();
            return View(model);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login), nameof(AccountController).Replace("Controller", ""));
        }

        [HttpGet("changePassword")]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }
        [HttpPost("changePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await GetUser();
                    if (user != null)
                    {
                        if (user.Password == model.OldPassword)
                        {
                            user.Password = model.NewPassword;
                            await _userService.UpdateUser(user);
                            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", ""));
                        }
                        ModelState.AddModelError(nameof(model.OldPassword), "Старый пароль введен неверно");
                    }
                }
                catch
                {
                    ModelState.AddModelError("", "Не удалось сменить пароль");
                }
            }
            return View(model);
        }
        [NonAction]
        private async Task Authenticate(string userLogin)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userLogin)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        [NonAction]
        private Task<User> GetUser()
        {
            var userLogin = User.Identity?.Name;
            return _userService.GetByLogin(userLogin);
        }
        [NonAction]
        private async Task<bool> Authorized()
        {
            var user = await GetUser();
            return user != null;
        }
    }
}
