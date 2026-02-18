using OfficeOpenXml;
using OfficeOpenXml.Style;
using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface IExcelExportService
    {
        byte[] ExportAuthorReport(List<AuthorStatisticModel> summary, List<NewsArticleModel> details, DateTime? startDate, DateTime? endDate);
        byte[] ExportCategoryReport(List<CategoryStatisticModel> summary, List<NewsArticleModel> details, DateTime? startDate, DateTime? endDate);
    }

    public class ExcelExportService : IExcelExportService
    {
        public ExcelExportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public byte[] ExportAuthorReport(List<AuthorStatisticModel> summary, List<NewsArticleModel> details, DateTime? startDate, DateTime? endDate)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Author Report");

            worksheet.Cells[1, 1].Value = "FU News Management - Articles by Author Report";
            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var dateRange = $"Period: {startDate?.ToString("MMM dd, yyyy") ?? "All Time"} - {endDate?.ToString("MMM dd, yyyy") ?? "Present"}";
            worksheet.Cells[2, 1].Value = dateRange;
            worksheet.Cells[2, 1, 2, 6].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[3, 1].Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}";
            worksheet.Cells[3, 1, 3, 6].Merge = true;
            worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var row = 5;
            worksheet.Cells[row, 1].Value = "Author";
            worksheet.Cells[row, 2].Value = "Total Articles";
            worksheet.Cells[row, 3].Value = "Active";
            worksheet.Cells[row, 4].Value = "Inactive";
            worksheet.Cells[row, 5].Value = "Last Article";
            worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;

            foreach (var item in summary)
            {
                row++;
                worksheet.Cells[row, 1].Value = item.AuthorName;
                worksheet.Cells[row, 2].Value = item.TotalArticles;
                worksheet.Cells[row, 3].Value = item.ActiveArticles;
                worksheet.Cells[row, 4].Value = item.InactiveArticles;
                worksheet.Cells[row, 5].Value = item.LastArticleDate;
            }

            row++;
            worksheet.Cells[row, 1].Value = "Total";
            worksheet.Cells[row, 2].Value = summary.Sum(x => x.TotalArticles);
            worksheet.Cells[row, 3].Value = summary.Sum(x => x.ActiveArticles);
            worksheet.Cells[row, 4].Value = summary.Sum(x => x.InactiveArticles);
            worksheet.Cells[row, 1, row, 5].Style.Font.Bold = true;

            row += 2;
            worksheet.Cells[row, 1].Value = "Article Details";
            worksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            worksheet.Cells[row, 1].Value = "Title";
            worksheet.Cells[row, 2].Value = "Author";
            worksheet.Cells[row, 3].Value = "Category";
            worksheet.Cells[row, 4].Value = "Created Date";
            worksheet.Cells[row, 5].Value = "View Count";
            worksheet.Cells[row, 6].Value = "Status";
            worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;

            foreach (var article in details)
            {
                row++;
                worksheet.Cells[row, 1].Value = article.NewsTitle;
                worksheet.Cells[row, 2].Value = article.CreatedByName;
                worksheet.Cells[row, 3].Value = article.CategoryName;
                worksheet.Cells[row, 4].Value = article.CreatedDate?.ToString("MMM dd, yyyy");
                worksheet.Cells[row, 5].Value = article.ViewCount;
                worksheet.Cells[row, 6].Value = article.NewsStatus == true ? "Active" : "Inactive";
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        public byte[] ExportCategoryReport(List<CategoryStatisticModel> summary, List<NewsArticleModel> details, DateTime? startDate, DateTime? endDate)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Category Report");

            worksheet.Cells[1, 1].Value = "FU News Management - Articles by Category Report";
            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var dateRange = $"Period: {startDate?.ToString("MMM dd, yyyy") ?? "All Time"} - {endDate?.ToString("MMM dd, yyyy") ?? "Present"}";
            worksheet.Cells[2, 1].Value = dateRange;
            worksheet.Cells[2, 1, 2, 6].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[3, 1].Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}";
            worksheet.Cells[3, 1, 3, 6].Merge = true;
            worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var row = 5;
            worksheet.Cells[row, 1].Value = "Category";
            worksheet.Cells[row, 2].Value = "Total Articles";
            worksheet.Cells[row, 3].Value = "Active";
            worksheet.Cells[row, 4].Value = "Inactive";
            worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;

            foreach (var item in summary)
            {
                row++;
                worksheet.Cells[row, 1].Value = item.CategoryName;
                worksheet.Cells[row, 2].Value = item.TotalArticles;
                worksheet.Cells[row, 3].Value = item.ActiveArticles;
                worksheet.Cells[row, 4].Value = item.InactiveArticles;
            }

            row++;
            worksheet.Cells[row, 1].Value = "Total";
            worksheet.Cells[row, 2].Value = summary.Sum(x => x.TotalArticles);
            worksheet.Cells[row, 3].Value = summary.Sum(x => x.ActiveArticles);
            worksheet.Cells[row, 4].Value = summary.Sum(x => x.InactiveArticles);
            worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;

            row += 2;
            worksheet.Cells[row, 1].Value = "Article Details";
            worksheet.Cells[row, 1].Style.Font.Bold = true;

            row++;
            worksheet.Cells[row, 1].Value = "Title";
            worksheet.Cells[row, 2].Value = "Author";
            worksheet.Cells[row, 3].Value = "Category";
            worksheet.Cells[row, 4].Value = "Created Date";
            worksheet.Cells[row, 5].Value = "View Count";
            worksheet.Cells[row, 6].Value = "Status";
            worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;

            foreach (var article in details)
            {
                row++;
                worksheet.Cells[row, 1].Value = article.NewsTitle;
                worksheet.Cells[row, 2].Value = article.CreatedByName;
                worksheet.Cells[row, 3].Value = article.CategoryName;
                worksheet.Cells[row, 4].Value = article.CreatedDate?.ToString("MMM dd, yyyy");
                worksheet.Cells[row, 5].Value = article.ViewCount;
                worksheet.Cells[row, 6].Value = article.NewsStatus == true ? "Active" : "Inactive";
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}