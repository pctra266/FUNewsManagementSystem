using DataAccess.Models;
using DataAccess.DTOs;

namespace BussinessLogic.Services
{
    public interface IReportService
    {
        Task<object> GetArticleStatisticsByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<CategoryReportDto> GetArticleStatisticsByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, bool? status = null);
        Task<AuthorReportDto> GetArticleStatisticsByAuthorAsync(DateTime? startDate = null, DateTime? endDate = null, bool? status = null);
        Task<StatusReportDto> GetArticleStatisticsByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<object> GetCategoryUsageStatisticsAsync();
        Task<object> GetDashboardStatisticsAsync();
        Task<object> GetMonthlyArticleStatsAsync(int year);
        Task<object> GetTopAuthorsAsync(int limit = 10);
        Task<object> GetTopCategoriesAsync(int limit = 10);
        Task<object> GetTagUsageStatisticsAsync();
        Task<byte[]> ExportToExcelAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}