using System;
using System.Collections.Generic;

namespace DataAccess.DTOs
{
    public class PeriodDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CategoryStatisticDto
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
        public DateTime? LatestArticle { get; set; }
    }

    public class CategoryReportDto
    {
        public PeriodDto Period { get; set; } = new PeriodDto();
        public List<CategoryStatisticDto> CategoryStatistics { get; set; } = new List<CategoryStatisticDto>();
    }

    public class AuthorStatisticDto
    {
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
        public DateTime? LatestArticle { get; set; }
        public DateTime? FirstArticle { get; set; }
    }

    public class AuthorReportDto
    {
        public PeriodDto Period { get; set; } = new PeriodDto();
        public List<AuthorStatisticDto> AuthorStatistics { get; set; } = new List<AuthorStatisticDto>();
    }

    public class StatusStatisticDto
    {
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
        public double ActivePercentage { get; set; }
        public double InactivePercentage { get; set; }
    }

    public class StatusReportDto
    {
        public PeriodDto Period { get; set; } = new PeriodDto();
        public StatusStatisticDto StatusStatistics { get; set; } = new StatusStatisticDto();
    }
}
