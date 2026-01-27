using DataAccess.Models;

namespace BussinessLogic.Services
{
    public interface IReportService
    {
        Task<object> GetArticleStatisticsByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<object> GetArticleStatisticsByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetArticleStatisticsByAuthorAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetArticleStatisticsByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetCategoryUsageStatisticsAsync();
        Task<object> GetDashboardStatisticsAsync();
        Task<object> GetMonthlyArticleStatsAsync(int year);
        Task<object> GetTopAuthorsAsync(int limit = 10);
        Task<object> GetTopCategoriesAsync(int limit = 10);
        Task<object> GetTagUsageStatisticsAsync();
    }
}