using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string email, string password);
        Task<UserInfo?> GetCurrentUserAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;

        public AuthService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var request = new { Email = email, Password = password };
            return await _apiClient.PostAsync<LoginResponse>("Auth/login", request);
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            return await _apiClient.GetAsync<UserInfo>("Auth/me");
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
    }

    public class UserInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsStaff { get; set; }
        public bool IsLecturer { get; set; }
    }
}