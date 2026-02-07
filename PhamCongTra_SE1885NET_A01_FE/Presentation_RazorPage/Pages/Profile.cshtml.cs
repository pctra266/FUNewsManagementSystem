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

        [BindProperty]
        public ProfileUpdateViewModel UpdateProfile { get; set; } = new ProfileUpdateViewModel();

        [BindProperty]
        public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Login");
            }

            // Only Staff (role = "1") can access profile
            if (userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied. Profile management is only available for Staff members.";

                return userRole switch
                {
                    "Admin" => RedirectToPage("/Admin/Dashboard"),
                    "2" => RedirectToPage("/News/Active"),
                    _ => RedirectToPage("/Login")
                };
            }

            //_apiService.SetAuthToken(token);

            try
            {
                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "Invalid user session. Please login again.";
                    return RedirectToPage("/Login");
                }

                var userAccount = await _apiService.GetByIdAsync<SystemAccountModel>("/odata/SystemAccounts", userId.Value);

                if (userAccount != null)
                {
                    CurrentUser = userAccount;

                    // Pre-populate the update form with only name
                    UpdateProfile = new ProfileUpdateViewModel
                    {
                        AccountName = userAccount.AccountName ?? ""
                    };
                }
                else
                {
                    TempData["ErrorMessage"] = "User profile not found. Please contact administrator.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error loading user profile.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            // Only Staff can update profile
            if (userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied. Profile updates are only allowed for Staff members.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var token = HttpContext.Session.GetString("AuthToken");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Invalid session. Please login again.";
                return RedirectToPage("/Login");
            }

            //_apiService.SetAuthToken(token!);

            try
            {
                // Get current account to preserve email
                var currentAccount = await _apiService.GetByIdAsync<SystemAccountModel>("/odata/SystemAccounts", userId.Value);

                if (currentAccount == null)
                {
                    TempData["ErrorMessage"] = "Failed to load current account information.";
                    return RedirectToPage();
                }

                // Update only name, keep email unchanged
                var updateData = new
                {
                    AccountName = UpdateProfile.AccountName,
                    AccountEmail = currentAccount.AccountEmail, // Keep original email
                    AccountRole = 1
                };

                var result = await _apiService.PutAsync<SystemAccountModel>("/odata/SystemAccounts", userId.Value, updateData);

                if (result != null)
                {
                    HttpContext.Session.SetString("UserName", UpdateProfile.AccountName);
                    TempData["SuccessMessage"] = "Profile name updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update profile.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while updating profile.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            // Only Staff can change password via profile
            if (userRole != "1")
            {
                TempData["ErrorMessage"] = "Access denied. Password changes are only allowed for Staff members.";
                return RedirectToPage();
            }

            // Validate password change model
            if (string.IsNullOrEmpty(ChangePassword.CurrentPassword) ||
                string.IsNullOrEmpty(ChangePassword.NewPassword) ||
                string.IsNullOrEmpty(ChangePassword.ConfirmPassword))
            {
                TempData["ErrorMessage"] = "All password fields are required.";
                return RedirectToPage();
            }

            if (ChangePassword.NewPassword != ChangePassword.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return RedirectToPage();
            }

            if (ChangePassword.NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "New password must be at least 6 characters long.";
                return RedirectToPage();
            }

            var token = HttpContext.Session.GetString("AuthToken");
            //_apiService.SetAuthToken(token!);

            try
            {
                var changePasswordData = new
                {
                    CurrentPassword = ChangePassword.CurrentPassword,
                    NewPassword = ChangePassword.NewPassword,
                    ConfirmPassword = ChangePassword.ConfirmPassword
                };

                var result = await _apiService.PostAsync<object>("/odata/SystemAccountsFunctions/ChangePassword", changePasswordData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Password changed successfully! Please login again.";
                    HttpContext.Session.Clear();
                    return RedirectToPage("/Login");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change password. Current password may be incorrect.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while changing password.";
            }

            return RedirectToPage();
        }
    }

    public class ProfileUpdateViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string AccountName { get; set; } = string.Empty;
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