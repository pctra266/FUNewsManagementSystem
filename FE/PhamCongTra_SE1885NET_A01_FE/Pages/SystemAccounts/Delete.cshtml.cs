using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhamCongTra_SE1885NET_A01_FE.Pages.SystemAccounts
{
    [Authorize(Roles = "ADMIN")]
    public class DeleteModel : PageModel
    {
        private readonly IAccountService _accountService;

        // 2. Inject Service
        public DeleteModel(IAccountService accountService)
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
                // 3. Gọi Service lấy thông tin Account theo ID
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

        public async Task<IActionResult> OnPostAsync(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // 4. Gọi Service thực hiện xóa
                await _accountService.DeleteAccountAsync(id.Value);

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // 5. Xử lý lỗi: Nếu xóa thất bại, load lại dữ liệu để hiện trang xác nhận
                ModelState.AddModelError(string.Empty, "Error deleting account: " + ex.Message);

                // Load lại để view không bị null
                SystemAccount = await _accountService.GetAccountByIdAsync(id.Value);

                return Page();
            }
        }
    }
}
