using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BusinessLogic.Services;
using System.Text.Json;

namespace Presentation_RazorPage.Pages.Admin.AuditLog
{
    public class IndexModel : PageModel
    {
        private readonly IApiService _apiService;

        public IndexModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        public List<AuditLogModel> AuditLogs { get; set; } = new List<AuditLogModel>();
        public List<UserFilterModel> Users { get; set; } = new List<UserFilterModel>();
        public List<string> EntityTypes { get; set; } = new List<string>();

        [BindProperty(SupportsGet = true)]
        public short? FilterUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterEntityType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AuthToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token) || userRole != "Admin")
            {
                return RedirectToPage("/Login");
            }

            //_apiService.SetAuthToken(token);

            try
            {
                // Build query string
                var queryParts = new List<string>();
                if (FilterUserId.HasValue)
                {
                    queryParts.Add($"userId={FilterUserId.Value}");
                }
                if (!string.IsNullOrWhiteSpace(FilterEntityType))
                {
                    queryParts.Add($"entityType={FilterEntityType}");
                }

                var query = queryParts.Any() ? "?" + string.Join("&", queryParts) : "";

                // Fetch audit logs
                var logs = await _apiService.GetAsync<AuditLogModel>($"/api/AuditLog{query}");
                AuditLogs = logs ?? new List<AuditLogModel>();

                // Get unique users for filter
                var allUsers = await _apiService.GetAsync<UserFilterModel>("/odata/SystemAccounts?$select=AccountId,AccountName,AccountEmail");
                Users = (allUsers ?? new List<UserFilterModel>())
                    .OrderBy(u => u.AccountName)
                    .ToList();

                // Get unique entity types from logs
                EntityTypes = AuditLogs
                    .Select(l => l.EntityName)
                    .Distinct()
                    .OrderBy(e => e)
                    .ToList();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading audit logs: {ex.Message}";
                AuditLogs = new List<AuditLogModel>();
                Users = new List<UserFilterModel>();
                EntityTypes = new List<string>();
            }

            return Page();
        }
    }

    public class AuditLogModel
    {
        public int LogId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }

        public string FormattedOldValues => FormatJson(OldValues);
        public string FormattedNewValues => FormatJson(NewValues);

        private string FormatJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "N/A";
            
            try
            {
                var obj = JsonSerializer.Deserialize<object>(json);
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }
    }

    public class UserFilterModel
    {
        public short AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
    }
}
