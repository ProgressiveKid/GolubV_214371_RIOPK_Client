using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CorporateRiskManagementSystem.Frontend.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        var cookieValue = Request.Cookies["CRMSAuth"];
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToPage("/AuthPages/Autorisation");
        }
        ViewData["username"] = User.Identity.Name;
        return Page();
    }
}
