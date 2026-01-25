using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.SystemAccounts
{
    [Authorize(Roles = "ADMIN")]
    public class DetailsModel : PageModel
    {
        private readonly IAccountService _accountService;

        // 4. Inject Service
        public DetailsModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public SystemAccount SystemAccount { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 5. Gọi Service lấy thông tin theo ID
                // Hàm GetAccountByIdAsync này đã được thêm vào Service ở bước trước (khi làm trang Delete)
                SystemAccount = await _accountService.GetAccountByIdAsync(id.Value);

                if (SystemAccount == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception)
            {
                // Xử lý lỗi (Log nếu cần)
                return NotFound();
            }
        }
    }
}
