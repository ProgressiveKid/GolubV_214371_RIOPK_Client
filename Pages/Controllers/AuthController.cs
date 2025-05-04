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

namespace CorporateRiskManagementSystem.Frontend.Pages.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        RiskDbContext db;
        public AuthController(RiskDbContext context)
        {
            db = context;
        }

        [HttpGet("Autorisation")]
        public IActionResult Autorisation()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Ваш код, который будет выполнен, если пользователь авторизован
                //var rooms = await db.Rooms.ToListAsync();
                return View();
            }
            else
            {
                // Ваш код, который будет выполнен, если пользователь не авторизован
                return View("Autorisation");
                // Пример перенаправления на страницу входа
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
                // Ищем пользователя по email и паролю
                User user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);
                if (user != null)
                {
                    Role curRole = Enum.Parse<Role>(user.Role);
                    // Здесь нужно будет выполнить аутентификацию, например, создать сессию или токен JWT
                    await Authenticate(model.Email, curRole);
                    // Ответ в формате JSON
                    return Json(new { success = true, redirectUrl = "https://localhost:7100" });
                }
                else
                {
                    // Если пользователь не найден, отправляем ошибку
                    return Json(new { success = false, message = "Некорректные логин и(или) пароль" });
                }
            }

            // Если модель невалидна
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
