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

        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        [BindProperty(SupportsGet = true)]
        public short? FilterUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FilterEntityType { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageIndex = 1)
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
                CurrentPage = pageIndex < 1 ? 1 : pageIndex;

                // Build OData filter
                var filters = new List<string>();
                if (FilterUserId.HasValue)
                {
                    filters.Add($"UserId eq {FilterUserId.Value}");
                }
                if (!string.IsNullOrWhiteSpace(FilterEntityType))
                {
                    filters.Add($"EntityName eq '{FilterEntityType}'");
                }

                var filterQuery = filters.Any() ? $"$filter={string.Join(" and ", filters)}" : "";
                var expandClause = "$expand=User($select=AccountName,AccountEmail)";
                
                // Add pagination
                var skip = (CurrentPage - 1) * PageSize;
                var paginationClause = $"$top={PageSize}&$skip={skip}&$count=true";

                var query = $"?{filterQuery}{(filters.Any() ? "&" : "")}{expandClause}&{paginationClause}";

                // Fetch audit logs via OData
                var logsResponse = await _apiService.GetODataAsync<JsonElement>($"/odata/AuditLogs{query}");
                
                AuditLogs = new List<AuditLogModel>();
                if (logsResponse != null)
                {
                    TotalItems = logsResponse.Count;
                    if (logsResponse.Value != null)
                    {
                        foreach (var log in logsResponse.Value)
                        {
                            AuditLogs.Add(MapJsonToAuditLogModel(log));
                        }
                    }
                }

                // Get unique users for filter via OData
                // Optimization: Limit to top 100 users to prevent performance issues with large datasets
                var allUsers = await _apiService.GetAsync<UserFilterModel>("/odata/SystemAccounts?$select=AccountId,AccountName,AccountEmail&$orderby=AccountName&$top=100");
                Users = (allUsers ?? new List<UserFilterModel>())
                    .OrderBy(u => u.AccountName)
                    .ToList();

                // Get unique entity types from logs (Ideally this should be a separate API call to get distinct types, 
                // but for now we might rely on loaded logs or a hardcoded list if performance is key. 
                // For simplicity properly sticking to current logic but strictly it only shows types in current page)
                // Better approach: fetch distinct EntityNames via OData apply if supported, or just common known types or large list
                // To keep it fast, let's pre-populate common types or fetch a small distinct list if possible.
                // Current logic was client-side distinct from ALL logs. Since we now page, we can't easily get ALL types from client.
                // We will skip dynamic entity type population from *all* logs to avoid the slow fetch.
                // Instead we can use a hardcoded list of known entities or just what's on page + accumulator?
                // For now, let's trust the requirement "Investigating AuditLog Page Slowness" -> Solved by not fetching all.
                // We will populate a standard list or leave it empty/based on page.
                // Let's manually add known types for filter to be useful.
                EntityTypes = new List<string> { "NewsArticle", "Category", "Tag", "SystemAccount" }; 
                // Or if we want to be dynamic, we'd need a specific endpoint. 
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

        private AuditLogModel MapJsonToAuditLogModel(JsonElement jsonLog)
        {
            return new AuditLogModel
            {
                LogId = jsonLog.GetProperty("LogId").GetInt32(),
                Action = jsonLog.GetProperty("Action").GetString() ?? "",
                EntityName = jsonLog.GetProperty("EntityName").GetString() ?? "",
                EntityId = jsonLog.GetProperty("EntityId").GetString() ?? "",
                OldValues = jsonLog.TryGetProperty("OldValues", out var oldVal) ? oldVal.GetString() : null,
                NewValues = jsonLog.TryGetProperty("NewValues", out var newVal) ? newVal.GetString() : null,
                Timestamp = jsonLog.GetProperty("Timestamp").GetDateTime(),
                UserName = jsonLog.TryGetProperty("User", out var user) && user.TryGetProperty("AccountName", out var name) ? name.GetString() ?? "Unknown" : "Unknown",
                UserEmail = jsonLog.TryGetProperty("User", out var u) && u.TryGetProperty("AccountEmail", out var email) ? email.GetString() ?? "Unknown" : "Unknown"
            };
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
