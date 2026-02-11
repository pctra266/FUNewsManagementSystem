using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ReportModel
    {
        public object? Data { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
public class RecentArticleModel
{
    public string NewsArticleId { get; set; } = string.Empty;
    public string? NewsTitle { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MonthlyStatsModel
{
    public int Year { get; set; }
    public List<MonthlyStatistic> MonthlyStatistics { get; set; } = new List<MonthlyStatistic>();
    public int YearTotal { get; set; }
    public int YearActive { get; set; }
    public int YearInactive { get; set; }
}

public class MonthlyStatistic
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
}

// Staff Dashboard Models
public class StaffDashboardStatisticsModel
{
    public int MyTotalArticles { get; set; }
    public int MyActiveArticles { get; set; }
    public int MyInactiveArticles { get; set; }
    public List<RecentArticleModel> MyRecentArticles { get; set; } = new List<RecentArticleModel>();
}

// Reports Models
public class ReportDashboardModel
{
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public int TotalCategories { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalTags { get; set; }
}

public class CategoryReportModel
{
    public PeriodModel Period { get; set; } = new PeriodModel();
    public List<CategoryStatisticModel> CategoryStatistics { get; set; } = new List<CategoryStatisticModel>();
}

public class AuthorReportModel
{
    public PeriodModel Period { get; set; } = new PeriodModel();
    public List<AuthorStatisticModel> AuthorStatistics { get; set; } = new List<AuthorStatisticModel>();
}

public class PeriodModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CategoryStatisticModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public DateTime? LatestArticle { get; set; }
    public double Percentage { get; set; }
}

public class AuthorStatisticModel
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int TotalArticles { get; set; }
    public int ActiveArticles { get; set; }
    public int InactiveArticles { get; set; }
    public DateTime? LatestArticle { get; set; }
    public DateTime? FirstArticle { get; set; }
    public string LastArticleDate => LatestArticle?.ToString("MMM dd, yyyy") ?? "Never";
}