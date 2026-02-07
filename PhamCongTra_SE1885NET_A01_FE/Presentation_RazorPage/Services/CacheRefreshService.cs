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

        public CacheRefreshService(IServiceProvider serviceProvider, IMemoryCache memoryCache, ILogger<CacheRefreshService> logger)
        {
            _serviceProvider = serviceProvider;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Initial delay to let app start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

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

                // Wait 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task RefreshCacheAsync()
        {
            // Create scope to resolve Scoped IApiService
            using (var scope = _serviceProvider.CreateScope())
            {
                var apiService = scope.ServiceProvider.GetRequiredService<IApiService>();

                try
                {
                    // 1. Fetch Categories
                    var categories = await apiService.GetAsync<CategoryModel>("/api/Categories");
                    if (categories != null)
                    {
                        _memoryCache.Set("OFFLINE_CACHE_/api/Categories", categories);
                    }

                    // 2. Fetch NewsArticles
                    var articles = await apiService.GetAsync<NewsArticleModel>("/api/NewsArticles");
                    if (articles != null)
                    {
                        _memoryCache.Set("OFFLINE_CACHE_/api/NewsArticles", articles);
                    }

                    // 3. Save to File
                    if (categories != null && articles != null)
                    {
                        var offlineData = new OfflineData
                        {
                            Categories = categories,
                            NewsArticles = articles,
                            LastUpdated = DateTime.Now
                        };

                        var json = JsonSerializer.Serialize(offlineData);
                        await File.WriteAllTextAsync(OfflineFilePath, json);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "API unavailable. Attempting to load from offline file.");

                    if (File.Exists(OfflineFilePath))
                    {
                        var json = await File.ReadAllTextAsync(OfflineFilePath);
                        var offlineData = JsonSerializer.Deserialize<OfflineData>(json);

                        if (offlineData != null)
                        {
                            _memoryCache.Set("OFFLINE_CACHE_/api/Categories", offlineData.Categories);
                            _memoryCache.Set("OFFLINE_CACHE_/api/NewsArticles", offlineData.NewsArticles);
                            _logger.LogInformation("Loaded data from offline file. Last updated: {time}", offlineData.LastUpdated);
                        }
                    }
                }
            }
        }
        
        // Load data on startup if API is down? 
        // We can check if Cache is empty and file exists, then load from file?
        // But ExecuteAsync calls RefreshCacheAsync. If API fails, RefreshCacheAsync fails or returns null.
        // We should add Logic to LoadFromFile if API fails.
        // But let's keep it simple as per plan: "Fetch ... Save to MemoryCache AND generic JSON file".
        // AND "Update GetAsync... catch -> Read from JSON file/Cache".
        
        // Wait, my ApiService implementation ONLY reads from MemoryCache.
        // So I MUST populate MemoryCache FROM FILE if API fails.
        
        // Revised RefreshCacheAsync:
        /*
        try {
           call API
           update Cache
           update File
        } catch {
           load File
           update Cache
        }
        */
    }

    public class OfflineData
    {
        public List<CategoryModel> Categories { get; set; } = new();
        public List<NewsArticleModel> NewsArticles { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}
