using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.SystemAccounts
{
    [Authorize(Roles = "ADMIN")]
    public class CreateModel : PageModel
    {
        private readonly IAccountService _accountService;

        // 4. Inject Service
        public CreateModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public SystemAccount SystemAccount { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 5. Gọi Service để tạo tài khoản
                // Service sẽ lo việc tính toán ID (nếu cần) và gọi xuống Repository
                await _accountService.CreateAccountAsync(SystemAccount);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 6. Xử lý lỗi
                ModelState.AddModelError(string.Empty, "Error creating system account: " + ex.Message);
                return Page();
            }
        }
    }
}
