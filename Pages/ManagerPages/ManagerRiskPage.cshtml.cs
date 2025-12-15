using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CorporateRiskManagementSystem.Frontend.Pages.ManagerPages
{
    public class ManagerRiskPage : PageModel
    {
        public void OnGet()
        {
            var userId = User.Identity.Name;
            ViewData["UserId"] = userId;
        }
    }
}
