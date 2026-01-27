using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation_RazorPage.Pages.Staff
{
    public class DashboardModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            // According to project requirements, Staff doesn't need a dashboard
            // Redirect Staff directly to their main work area - News Articles Management
            if (userRole == "1") // Staff
            {
                return RedirectToPage("/Staff/NewsArticles/Index");
            }
            else if (userRole == "2") // Lecturer
            {
                // Lecturer can only read and search articles
                return RedirectToPage("/News/Active");
            }
            else // Admin or unknown role
            {
                return RedirectToPage("/Admin/Dashboard");
            }
        }
    }
}