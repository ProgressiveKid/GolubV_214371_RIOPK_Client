using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CorporateRiskManagementSystem.Frontend.Pages.AuditorPages
{
    public class ReportPage : PageModel
    {
        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/AuthPages/Autorisation");
            }
            ViewData["username"] = User.Identity.Name;
            return Page();
        }
    }
}
