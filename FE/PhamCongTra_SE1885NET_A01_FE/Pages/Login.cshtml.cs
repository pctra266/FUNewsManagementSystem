using BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;

namespace PhamCongTra_SE1885NET_A01_FE.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly DataAccess.Models.IApiClient _apiClient;

        public LoginModel(IAuthService authService, DataAccess.Models.IApiClient apiClient)
        {
            _authService = authService;
            _apiClient = apiClient;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Check if already logged in
            var token = HttpContext.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                Response.Redirect("/Dashboard");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return Page();
            }

            try
            {
                var result = await _authService.LoginAsync(Email, Password);

                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    ErrorMessage = "Invalid email or password.";
                    return Page();
                }

                // Store authentication info in session
                HttpContext.Session.SetString("AuthToken", result.Token);
                HttpContext.Session.SetString("UserName", result.UserName);
                HttpContext.Session.SetString("Role", result.Role);

                // Set token in API client for subsequent requests
                _apiClient.SetAuthToken(result.Token);

                // Redirect based on role
                if (result.Role == "ADMIN")
                {
                    return RedirectToPage("/Dashboard/Index");
                }
                else if (result.Role == "STAFF")
                {
                    return RedirectToPage("/NewsArticles/Index");
                }
                else
                {
                    return RedirectToPage("/NewsArticles/Index");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                return Page();
            }
        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}