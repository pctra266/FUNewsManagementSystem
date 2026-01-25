using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace PhamCongTra_SE1885NET_A01_FE.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        // 1. Dùng Service gọi API thay vì Service gọi DB
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IAccountService accountService, IConfiguration configuration)
        {
            _logger = logger;
            _accountService = accountService;
            _configuration = configuration;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Nếu đã login rồi thì đá sang trang chủ luôn
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/News/Index");
            }
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return Page();
            }

            // --- 1. CHECK ADMIN HARDCODE (Giữ nguyên logic của bạn) ---
            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];

            if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword) &&
                Email == adminEmail && Password == adminPassword)
            {
                await SignInUser(new SystemAccount
                {
                    AccountId = 0,
                    AccountEmail = adminEmail,
                    AccountName = "System Administrator",
                    AccountRole = 1 // Giả sử 1 là Admin
                });
                return RedirectToPage("/News/Index"); // Hoặc trang Admin
            }

            // --- 2. GỌI API ĐỂ LOGIN ---
            // Thay vì gọi DB, ta gọi hàm LoginAsync từ Service (đã viết ở bước trước)
            var user = await _accountService.LoginAsync(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Nếu Active == false thì chặn (Optional)
            // if (user.AccountRole == 3) { ... }

            // Đăng nhập thành công -> Tạo Cookie
            await SignInUser(user);

            // Phân quyền chuyển hướng
            return user.AccountRole == 1 // Giả sử 1 là Admin/Staff
                ? RedirectToPage("/News/Index")
                : RedirectToPage("/News/Index");
        }

        // --- GOOGLE LOGIN ---

        public IActionResult OnGetGoogle(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("/Index", "ClaimRole", new { returnUrl = Uri.EscapeDataString(returnUrl) })
            };
            // Đảm bảo Program.cs đã add Google Auth
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        public async Task<IActionResult> OnGetClaimRoleAsync(string returnUrl = "/")
        {
            // Lấy thông tin từ Google trả về
            var googleUser = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            var email = googleUser.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = googleUser.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToPage("/Index");

            // --- LOGIC XỬ LÝ VỚI API ---

            // 1. Lấy danh sách user từ API để check xem email này tồn tại chưa
            // (Lý tưởng nhất là Backend API có endpoint GetByEmail, ở đây dùng tạm GetAll rồi lọc)
            var allAccounts = await _accountService.GetAccountsAsync();
            var existingUser = allAccounts.FirstOrDefault(a => a.AccountEmail == email);

            if (existingUser == null)
            {
                // 2. Nếu chưa có -> Gọi API tạo mới (Register)
                // Lưu ý: ID để 0 để Backend tự gen (Identity)
                var newUser = new SystemAccount
                {
                    AccountId = 0,
                    AccountEmail = email,
                    AccountName = name,
                    AccountRole = 2, // Mặc định là User thường
                    AccountPassword = "@1", // Mật khẩu ngẫu nhiên hoặc mặc định
                    // IsActive = true
                };

                // Bạn cần bổ sung hàm RegisterAsync/CreateAsync vào IAccountService
                // await _accountService.RegisterAsync(newUser); 

                // Giả lập sau khi tạo xong thì gán user hiện tại là user mới
                existingUser = newUser;
            }

            // 3. Đăng nhập vào hệ thống Cookie của web mình
            await SignInUser(existingUser);

            return RedirectToPage("/News/Index");
        }

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Xóa cả Session
            return RedirectToPage("/Account/Login"); // Quay về trang Login
        }

        // Hàm phụ để tạo Cookie (Tránh lặp code)
        private async Task SignInUser(SystemAccount user)
        {
            var roleString = user.AccountRole == 1 ? "ADMIN" : "STAFF";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                new Claim(ClaimTypes.Email, user.AccountEmail ?? ""),
                new Claim(ClaimTypes.Name, user.AccountName ?? ""),
                new Claim(ClaimTypes.Role, roleString)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Lưu ID vào Session để tiện dùng ở các trang khác
            HttpContext.Session.SetString("AccountId", user.AccountId.ToString());

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
