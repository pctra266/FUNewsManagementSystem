using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;

namespace Presentation_RazorPage.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IApiService _apiService;

        public LoginModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public LoginViewModel LoginData { get; set; } = new LoginViewModel();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Clear any existing session
            HttpContext.Session.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _apiService.LoginAsync(LoginData);
                
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    // Store token and user info in session
                    HttpContext.Session.SetString("AuthToken", response.Token);
                    HttpContext.Session.SetString("RefreshToken", response.RefreshToken);
                    HttpContext.Session.SetString("UserName", response.Account.AccountName ?? "User");
                    HttpContext.Session.SetString("UserRole", response.Account.AccountRole?.ToString() ?? "Admin");
                    HttpContext.Session.SetString("UserEmail", response.Account.AccountEmail ?? "");
                    HttpContext.Session.SetInt32("UserId", response.Account.AccountId);
                    HttpContext.Session.SetString("TokenExpiresAt", response.ExpiresAt.ToString("o"));

                    // Set auth token for API service - Removed as per refactor
                    // _apiService.SetAuthToken(response.Token);

                    // Redirect based on user role according to project requirements
                    return response.Account.AccountRole switch
                    {
                        _ => RedirectToPage("/News/Index")            // Admin - go to account management
                    };
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please try again.";
                    ModelState.AddModelError(string.Empty, ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while logging in. Please try again later.";
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            return Page();
        }
    }
}