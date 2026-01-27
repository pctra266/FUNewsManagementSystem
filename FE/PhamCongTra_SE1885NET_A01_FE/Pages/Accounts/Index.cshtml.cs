using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.Accounts
{
    public class IndexModel : PageModel
    {
        private readonly IAccountService _accountService;

        public IndexModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public List<SystemAccount> Accounts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchKeyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? FilterRole { get; set; }

        public async Task OnGetAsync()
        {
            Accounts = await _accountService.SearchAsync(SearchKeyword, FilterRole);
        }

        public async Task<IActionResult> OnPostDeleteAsync(short id)
        {
            try
            {
                await _accountService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Account deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Cannot delete account: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}