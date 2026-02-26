using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;
using System.Net.Http;

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
                    HttpContext.Session.SetString("AuthToken", response.Token);
                    HttpContext.Session.SetString("RefreshToken", response.RefreshToken);
                    HttpContext.Session.SetString("UserName", response.Account.AccountName ?? "User");
                    HttpContext.Session.SetString("UserRole", response.Account.AccountRole?.ToString() ?? "Admin");
                    HttpContext.Session.SetString("UserEmail", response.Account.AccountEmail ?? string.Empty);
                    HttpContext.Session.SetInt32("UserId", response.Account.AccountId);
                    HttpContext.Session.SetString("TokenExpiresAt", response.ExpiresAt.ToString("o"));

                    return response.Account.AccountRole switch
                    {
                        _ => RedirectToPage("/News/Index")
                    };
                }

                ErrorMessage = "Invalid email or password. Please try again.";
                return Page();
            }
            catch (HttpRequestException)
            {
                ErrorMessage = "Authentication service is unavailable after multiple retries. Please try again later.";
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred while logging in. Please try again later.";
                return Page();
            }

            return Page();
        }
    }
}