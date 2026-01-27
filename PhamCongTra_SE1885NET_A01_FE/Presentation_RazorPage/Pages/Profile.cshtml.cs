using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;
using System.ComponentModel.DataAnnotations;

namespace Presentation_RazorPage.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly IApiService _apiService;

        public ProfileModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public SystemAccountModel? CurrentUser { get; set; }
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public DateTime? LastArticleDate { get; set; }

        [BindProperty]
        public ProfileUpdateViewModel UpdateProfile { get; set; } = new ProfileUpdateViewModel();

        [BindProperty]
        public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication
            var token = HttpContext.Session.GetString("AuthToken");
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (string.IsNullOrEmpty(token) || !userId.HasValue)
            {
                return RedirectToPage("/Login");
            }

            _apiService.SetAuthToken(token);

            try
            {
                // Get current user details
                CurrentUser = await _apiService.GetByIdAsync<SystemAccountModel>("/odata/SystemAccounts", userId.Value);
                
                if (CurrentUser != null)
                {
                    // Populate update form with current data
                    UpdateProfile = new ProfileUpdateViewModel
                    {
                        AccountName = CurrentUser.AccountName ?? "",
                        AccountEmail = CurrentUser.AccountEmail ?? ""
                    };

                    // Get user's article statistics (if staff/lecturer)
                    if (CurrentUser.AccountRole == 1 || CurrentUser.AccountRole == 2)
                    {
                        try
                        {
                            var articlesResponse = await _apiService.GetAsync<NewsArticleModel>($"/odata/NewsArticlesFunctions/ByAuthor?authorId={userId}");
                            var articles = articlesResponse ?? new List<NewsArticleModel>();
                            
                            TotalArticles = articles.Count;
                            PublishedArticles = articles.Count(a => a.NewsStatus == true);
                            LastArticleDate = articles.OrderByDescending(a => a.CreatedDate).FirstOrDefault()?.CreatedDate;
                        }
                        catch
                        {
                            // Ignore errors for article statistics
                        }
                    }
                }
            }
            catch (Exception)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload data
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                var userId = HttpContext.Session.GetInt32("UserId");
                _apiService.SetAuthToken(token!);

                var updateData = new
                {
                    AccountName = UpdateProfile.AccountName,
                    AccountEmail = UpdateProfile.AccountEmail,
                    AccountRole = CurrentUser?.AccountRole ?? 1
                };

                var result = await _apiService.PutAsync<SystemAccountModel>("/odata/SystemAccounts", userId!.Value, updateData);

                if (result != null)
                {
                    // Update session data
                    HttpContext.Session.SetString("UserName", UpdateProfile.AccountName);
                    HttpContext.Session.SetString("UserEmail", UpdateProfile.AccountEmail);
                    
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update profile. Email might already exist.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // Validate password change form separately
            if (string.IsNullOrEmpty(ChangePassword.CurrentPassword) ||
                string.IsNullOrEmpty(ChangePassword.NewPassword) ||
                string.IsNullOrEmpty(ChangePassword.ConfirmPassword))
            {
                ModelState.AddModelError("ChangePassword.CurrentPassword", "All password fields are required");
            }
            else if (ChangePassword.NewPassword != ChangePassword.ConfirmPassword)
            {
                ModelState.AddModelError("ChangePassword.ConfirmPassword", "Passwords do not match");
            }
            else if (ChangePassword.NewPassword.Length < 6)
            {
                ModelState.AddModelError("ChangePassword.NewPassword", "Password must be at least 6 characters");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload data
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                _apiService.SetAuthToken(token!);

                var changePasswordData = new
                {
                    CurrentPassword = ChangePassword.CurrentPassword,
                    NewPassword = ChangePassword.NewPassword
                };

                // Use the change password endpoint from Functions controller
                var response = await _apiService.PostAsync<object>("/odata/SystemAccountsFunctions/ChangePassword", changePasswordData);

                if (response != null)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                    ChangePassword = new ChangePasswordViewModel(); // Clear form
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change password. Current password may be incorrect.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while changing your password.";
            }

            return RedirectToPage();
        }
    }

    public class ProfileUpdateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string AccountName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
        public string AccountEmail { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [StringLength(70, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 70 characters")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}