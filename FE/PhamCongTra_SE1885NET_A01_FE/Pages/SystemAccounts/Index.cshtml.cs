using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.SystemAccounts
{
    [Authorize(Roles = "ADMIN")]
    public class IndexModel : PageModel
    {
        private readonly IAccountService _accountService;

        // 4. Inject Service qua Constructor
        public IndexModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IList<SystemAccount> SystemAccounts { get; set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                // 5. Gọi Service lấy danh sách
                // Hàm này sẽ gọi xuống Repository -> Repository gọi API GET /api/SystemAccounts
                SystemAccounts = await _accountService.GetAccountsAsync();
            }
            catch (Exception ex)
            {
                // 6. Xử lý lỗi
                SystemAccounts = new List<SystemAccount>();
                ModelState.AddModelError(string.Empty, "Error retrieving system accounts: " + ex.Message);
            }
        }
    }
}
