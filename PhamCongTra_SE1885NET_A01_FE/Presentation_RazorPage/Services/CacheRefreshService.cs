using BusinessLogic.Services;
using DataAccess.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Presentation_RazorPage.Services
{
    public class CacheRefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheRefreshService> _logger;

        private const string OfflineFilePath = "offline_data.json";
        private const string TagsEndpoint = "/odata/Tags";
        private const string CategoriesEndpoint = "/odata/CategoriesFunctions/Active";
        private const string SystemAccountsEndpoint = "/odata/SystemAccounts";
        private const string DashboardEndpoint = "/odata/NewsArticles/Default.Dashboard()";
        private const string NewsExpandClause = "$expand=Category($select=CategoryName),CreatedBy($select=AccountName),Tags,NewsArticleImages";
        private const int PublicTrendingCount = 4;
        private const int StaffTrendingCount = 5;
        private static readonly string ActiveNewsEndpoint = $"/odata/NewsArticles/Default.GetActive()?{NewsExpandClause}";
        private static readonly string PublicTrendingEndpoint = $"/odata/NewsArticles/Default.Trending(top={PublicTrendingCount})";
        private static readonly string StaffTrendingEndpoint = $"/odata/NewsArticles/Default.Trending(top={StaffTrendingCount})";
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        public CacheRefreshService(IServiceProvider serviceProvider, IMemoryCache memoryCache, ILogger<CacheRefreshService> logger)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("CacheRefreshService running at: {time}", DateTimeOffset.Now);

                try
                {
                    await RefreshCacheAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing cache");
                }

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task RefreshCacheAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();

            try
            {
                _logger.LogInformation("Starting full cache warm-up...");

                var categories = await apiService.GetAsync<CategoryModel>(CategoriesEndpoint);
                SetCacheEntry(CategoriesEndpoint, categories);

                var tags = await apiService.GetAsync<TagModel>(TagsEndpoint);
                SetCacheEntry(TagsEndpoint, tags);

                var activeArticles = await apiService.GetAsync<NewsArticleModel>(ActiveNewsEndpoint);
                SetCacheEntry(ActiveNewsEndpoint, activeArticles);

                var publicTrending = await apiService.GetAsync<NewsArticleModel>(PublicTrendingEndpoint);
                SetCacheEntry(PublicTrendingEndpoint, publicTrending);

                var staffTrending = await apiService.GetAsync<NewsArticleModel>(StaffTrendingEndpoint);
                SetCacheEntry(StaffTrendingEndpoint, staffTrending);

                var dashboard = await apiService.GetByIdAsync<DashboardStatisticsModel>(DashboardEndpoint);
                SetCacheEntry(DashboardEndpoint, dashboard);

                var systemAccounts = await apiService.GetAsync<SystemAccountModel>(SystemAccountsEndpoint);
                SetCacheEntry(SystemAccountsEndpoint, systemAccounts);

                var offlineData = new OfflineData
                {
                    Categories = categories ?? new(),
                    NewsArticles = activeArticles ?? new(),
                    Tags = tags ?? new(),
                    TrendingArticles = publicTrending ?? new(),
                    StaffTrendingArticles = staffTrending ?? new(),
                    DashboardStats = dashboard,
                    SystemAccounts = systemAccounts ?? new(),
                    LastUpdated = DateTime.Now
                };

                var json = JsonSerializer.Serialize(offlineData, JsonOptions);
                await File.WriteAllTextAsync(OfflineFilePath, json);
                _logger.LogInformation("Cache warm-up completed and saved to offline file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API unavailable during warm-up. Attempting to load from offline file.");

                if (!File.Exists(OfflineFilePath))
                {
                    return;
                }

                var json = await File.ReadAllTextAsync(OfflineFilePath);
                var offlineData = JsonSerializer.Deserialize<OfflineData>(json);

                if (offlineData == null)
                {
                    return;
                }

                SetCacheEntry(CategoriesEndpoint, offlineData.Categories);
                SetCacheEntry(TagsEndpoint, offlineData.Tags);
                SetCacheEntry(ActiveNewsEndpoint, offlineData.NewsArticles);
                SetCacheEntry(PublicTrendingEndpoint, offlineData.TrendingArticles);
                SetCacheEntry(StaffTrendingEndpoint, offlineData.StaffTrendingArticles);
                if (offlineData.DashboardStats != null)
                {
                    SetCacheEntry(DashboardEndpoint, offlineData.DashboardStats);
                }
                SetCacheEntry(SystemAccountsEndpoint, offlineData.SystemAccounts);

                _logger.LogInformation("Loaded cached payloads from offline file. Last updated: {time}", offlineData.LastUpdated);
            }
        }

        private static string BuildCacheKey(string endpoint) => $"OFFLINE_CACHE_{endpoint}";

        private void SetCacheEntry(string endpoint, object? payload)
        {
            if (payload == null)
            {
                return;
            }

            _memoryCache.Set(BuildCacheKey(endpoint), payload);
        }
    }

    public class OfflineData
    {
        public List<CategoryModel> Categories { get; set; } = new();
        public List<NewsArticleModel> NewsArticles { get; set; } = new();
        public List<TagModel> Tags { get; set; } = new();
        public List<NewsArticleModel> TrendingArticles { get; set; } = new();
        public List<NewsArticleModel> StaffTrendingArticles { get; set; } = new();
        public DashboardStatisticsModel? DashboardStats { get; set; }
        public List<SystemAccountModel> SystemAccounts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}
