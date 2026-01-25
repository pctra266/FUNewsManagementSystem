using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.SystemAccounts
{
    [Authorize(Roles = "ADMIN")]
    public class EditModel : PageModel
    {
        private readonly IAccountService _accountService;

        // 4. Inject Service
        public EditModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [BindProperty]
        public SystemAccount SystemAccount { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 5. Gọi Service lấy thông tin (Hàm này đã có từ bước làm trang Details/Delete)
                SystemAccount = await _accountService.GetAccountByIdAsync(id.Value);

                if (SystemAccount == null)
                {
                    return NotFound();
                }

                return Page();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 6. Gọi Service để cập nhật
                // Bạn cần đảm bảo Service đã có hàm UpdateAccountAsync (xem hướng dẫn bên dưới)
                await _accountService.UpdateAccountAsync(SystemAccount);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 7. Xử lý lỗi
                ModelState.AddModelError(string.Empty, "Error updating system account: " + ex.Message);
                return Page();
            }
        }
    }
}
