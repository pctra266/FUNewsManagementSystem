using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface IAccountService
    {
        Task<List<SystemAccount>> GetAllAsync();
        Task<SystemAccount?> GetByIdAsync(short id);
        Task<List<SystemAccount>> SearchAsync(string? keyword, short? role);
        Task CreateAsync(SystemAccount account);
        Task UpdateAsync(SystemAccount account);
        Task DeleteAsync(short id);
        Task ChangePasswordAsync(short id, string currentPassword, string newPassword);
    }

    public class AccountService : IAccountService
    {
        private readonly IApiClient _apiClient;

        public AccountService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<List<SystemAccount>> GetAllAsync()
        {
            return await _apiClient.GetAsync<List<SystemAccount>>("SystemAccounts")
                   ?? new List<SystemAccount>();
        }

        public async Task<SystemAccount?> GetByIdAsync(short id)
        {
            return await _apiClient.GetAsync<SystemAccount>($"SystemAccounts/{id}");
        }

        public async Task<List<SystemAccount>> SearchAsync(string? keyword, short? role)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");

            if (role.HasValue)
                queryParams.Add($"role={role.Value}");

            var query = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(query)
                ? "SystemAccounts/Search"
                : $"SystemAccounts/Search?{query}";

            return await _apiClient.GetAsync<List<SystemAccount>>(endpoint)
                   ?? new List<SystemAccount>();
        }

        public async Task CreateAsync(SystemAccount account)
        {
            await _apiClient.PostAsync<SystemAccount>("SystemAccounts", account);
        }

        public async Task UpdateAsync(SystemAccount account)
        {
            await _apiClient.PutAsync<object>($"SystemAccounts/{account.AccountId}", account);
        }

        public async Task DeleteAsync(short id)
        {
            await _apiClient.DeleteAsync($"SystemAccounts/{id}");
        }

        public async Task ChangePasswordAsync(short id, string currentPassword, string newPassword)
        {
            var request = new { CurrentPassword = currentPassword, NewPassword = newPassword };
            await _apiClient.PutAsync<object>($"SystemAccounts/{id}/ChangePassword", request);
        }
    }
}