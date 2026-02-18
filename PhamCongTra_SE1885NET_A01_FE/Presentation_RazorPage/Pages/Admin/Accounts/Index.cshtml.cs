using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccess.Models;
using BusinessLogic.Services;
using System.ComponentModel.DataAnnotations;

namespace Presentation_RazorPage.Pages.Admin.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<SystemAccountModel> Accounts { get; set; } = new List<SystemAccountModel>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public int? RoleFilter { get; set; }

        [BindProperty]
        public SystemAccountCreateModel CreateAccount { get; set; } = new SystemAccountCreateModel();

        [BindProperty]
        public AdminChangePasswordViewModel ChangePassword { get; set; } = new AdminChangePasswordViewModel();

        public int TotalAccounts { get; set; }
        public int StaffAccounts { get; set; }
        public int LecturerAccounts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication and authorization
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            //_apiService.SetAuthToken(token);

            try
            {
                // Get all accounts
                var accountsResponse = await _apiService.GetAsync<SystemAccountModel>("/odata/SystemAccounts");
                var allAccounts = accountsResponse ?? new List<SystemAccountModel>();

                // Apply filters
                Accounts = allAccounts;

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    try
                    {
                        // Use search endpoint from Functions controller
                        var searchResponse = await _apiService.GetAsync<SystemAccountModel>($"/odata/SystemAccountsFunctions/Search?name={Uri.EscapeDataString(SearchTerm)}");
                        Accounts = searchResponse ?? new List<SystemAccountModel>();
                    }
                    catch (Exception)
                    {
                        Accounts = new List<SystemAccountModel>();
                    }
                }

                if (RoleFilter.HasValue)
                {
                    Accounts = Accounts.Where(a => a.AccountRole == RoleFilter).ToList();
                }

                // Calculate statistics
                TotalAccounts = allAccounts.Count;
                StaffAccounts = allAccounts.Count(a => a.AccountRole == 1);
                LecturerAccounts = allAccounts.Count(a => a.AccountRole == 2);
            }
            catch (Exception)
            {
                Accounts = new List<SystemAccountModel>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // Reload data
                return Page();
            }

            try
            {
                var token = HttpContext.Session.GetString("AuthToken");
                //_apiService.SetAuthToken(token!);

                var newAccount = new SystemAccountModel
                {
                    AccountName = CreateAccount.AccountName,
                    AccountEmail = CreateAccount.AccountEmail,
                    AccountRole = CreateAccount.AccountRole
                };

                // For demo purposes, we'll use a simple request
                var result = await _apiService.PostAsync<SystemAccountModel>("/odata/SystemAccounts", new {
                    AccountName = CreateAccount.AccountName,
                    AccountEmail = CreateAccount.AccountEmail,
                    AccountPassword = CreateAccount.AccountPassword,
                    AccountRole = CreateAccount.AccountRole
                });

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Account created successfully!";
                    CreateAccount = new SystemAccountCreateModel(); // Reset form
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create account. Email might already exist.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the account.");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(short id)
        {
            var token = HttpContext.Session.GetString("AuthToken");
            //_apiService.SetAuthToken(token!);

            try
            {
                var success = await _apiService.DeleteAsync("/odata/SystemAccounts", id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Account deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete account. Account may have created articles.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the account.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(short AccountId, string AccountName, string AccountEmail, int AccountRole)
        {
            Console.WriteLine($"=== UPDATE ACCOUNT REQUEST ===");
            Console.WriteLine($"AccountId: {AccountId}");
            Console.WriteLine($"AccountName: {AccountName}");
            Console.WriteLine($"AccountEmail: {AccountEmail}");
            Console.WriteLine($"AccountRole: {AccountRole}");

            // Validate input
            if (string.IsNullOrEmpty(AccountName) || string.IsNullOrEmpty(AccountEmail))
            {
                TempData["ErrorMessage"] = "Account name and email are required.";
                return RedirectToPage();
            }

            if (AccountRole < 1 || AccountRole > 2)
            {
                TempData["ErrorMessage"] = "Invalid role selected.";
                return RedirectToPage();
            }

            var token = HttpContext.Session.GetString("AuthToken");
            //_apiService.SetAuthToken(token!);

            try
            {
                var updateData = new
                {
                    AccountName = AccountName,
                    AccountEmail = AccountEmail,
                    AccountRole = AccountRole
                };

                Console.WriteLine($"Calling PUT /odata/SystemAccounts({AccountId}) with data: {System.Text.Json.JsonSerializer.Serialize(updateData)}");

                var result = await _apiService.PutAsync<SystemAccountModel>("/odata/SystemAccounts", AccountId, updateData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Account updated successfully!";
                    Console.WriteLine("? Account updated successfully");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update account. Email might already exist.";
                    Console.WriteLine("? Account update failed - result is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Account update exception: {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred while updating the account: {ex.Message}";
            }

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(ChangePassword, nameof(ChangePassword)))
            {
                Console.WriteLine("--------------------------------------------");
                Console.WriteLine("----------------FailRui03----------------------------");
                Console.WriteLine("--------------------------------------------");
                await OnGetAsync();
                return Page();
            }

            if (ChangePassword.NewPassword != ChangePassword.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return RedirectToPage();
            }

            try
            {
                var changePasswordData = new
                {
                    AccountId = ChangePassword.AccountId,
                    CurrentPassword = ChangePassword.CurrentPassword,
                    NewPassword = ChangePassword.NewPassword,
                    ConfirmPassword = ChangePassword.ConfirmPassword
                };

                var result = await _apiService.PostAsync<object>("/odata/SystemAccountsFunctions/AdminChangePassword", changePasswordData);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Password changed successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change password. Current password may be incorrect.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToPage();
        }
    }

    public class SystemAccountCreateModel
    {
        [Required(ErrorMessage = "Account name is required")]
        [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
        public string AccountName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(70, ErrorMessage = "Email cannot exceed 70 characters")]
        public string AccountEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(70, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 70 characters")]
        public string AccountPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Role is required")]
        [Range(1, 2, ErrorMessage = "Role must be 1 (Staff) or 2 (Lecturer)")]
        public int AccountRole { get; set; }
    }

    public class AdminChangePasswordViewModel
    {
        [Required]
        public short AccountId { get; set; }

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