using System.Security.Claims;
using CorporateRiskManagementSystemBack.Data;
using CorporateRiskManagementSystemBack.Domain.Entites;
using CorporateRiskManagementSystemBack.Domain.Entites.Enums;
using CorporateRiskManagementSystemBack.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CorporateRiskManagementSystemBack.Controllers;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CorporateRiskManagementSystem.Frontend.Pages.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        RiskDbContext db;
        private readonly HttpClient _httpClient;
        public AuthController(RiskDbContext context, HttpClient httpClient)
        {
            db = context;
            _httpClient = httpClient;
        }

        [HttpGet("Autorisation")]
        public IActionResult Autorisation()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return View("Autorisation");
            }
        }

        [HttpGet("RedirectToLoginView")]
        public IActionResult RedirectToLoginView()
        {
            return Redirect("https://localhost:7100/Auth/Login"); // Razor View во фронте
        }

        [HttpPost("Autorisation")]
        public async Task<IActionResult> Autorisation([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);
                if (user != null)
                {
                    Role curRole = Enum.Parse<Role>(user.Role);
                    await Authenticate(model.Email, curRole);
                    return Json(new { success = true, redirectUrl = "https://localhost:7100" });
                }
                else
                {
                    return Json(new { success = false, message = "Некорректные логин и(или) пароль" });
                }
            }

            return Json(new { success = false, message = "Неверные данные" });
        }

        [HttpGet("Registration")]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration([FromBody] RegisterViewModel userData)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
                if (user == null)
                {
                    db.Users.Add(new User
                    {
                        Email = userData.Email,
                        PasswordHash = userData.Password,
                        Role = Role.Auditor.ToString(),
                        FullName = userData.FirstName,
                        Username = userData.LastName,
                    });

                    await db.SaveChangesAsync();
                    await Authenticate(userData.Email, Role.Auditor);

                    return Ok(new
                    {
                        success = true,
                        redirectUrl = "/AuthPages/Autorisation"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Пользователь с таким Email уже существует"
                    });
                }
            }

            // Собрать ошибки из ModelState
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = errors.FirstOrDefault() ?? "Ошибка валидации"
            });
        }

        private async Task Authenticate(string userName, Role role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName),
                new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role.ToString())
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            // Создание куки с AuthenticationProperties
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddYears(1)
            });
        }
    }
}
