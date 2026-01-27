using OfficeOpenXml;
using OfficeOpenXml.Style;
using DataAccess.Models;

namespace BusinessLogic.Services
{
    public interface IExcelExportService
    {
        byte[] ExportDashboardReport(DashboardStatisticsModel data, DateTime? startDate, DateTime? endDate);
        byte[] ExportCategoryReport(List<CategoryStatisticModel> data, DateTime? startDate, DateTime? endDate);
        byte[] ExportAuthorReport(List<AuthorStatisticModel> data, DateTime? startDate, DateTime? endDate);
        byte[] ExportMonthlyReport(List<MonthlyStatistic> data, int year);
    }

    public class ExcelExportService : IExcelExportService
    {
        public ExcelExportService()
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public byte[] ExportDashboardReport(DashboardStatisticsModel data, DateTime? startDate, DateTime? endDate)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Dashboard Report");

            // Set title
            worksheet.Cells[1, 1].Value = "FU News Management - Dashboard Report";
            worksheet.Cells[1, 1, 1, 4].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Set date range
            var dateRange = $"Period: {startDate?.ToString("MMM dd, yyyy") ?? "All Time"} - {endDate?.ToString("MMM dd, yyyy") ?? "Present"}";
            worksheet.Cells[2, 1].Value = dateRange;
            worksheet.Cells[2, 1, 2, 4].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.Font.Italic = true;

            // Add generated timestamp
            worksheet.Cells[3, 1].Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}";
            worksheet.Cells[3, 1, 3, 4].Merge = true;
            worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[3, 1].Style.Font.Size = 10;

            // Add summary statistics
            var row = 5;
            worksheet.Cells[row, 1].Value = "Metric";
            worksheet.Cells[row, 2].Value = "Value";
            worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;

            var metrics = new Dictionary<string, int>
            {
                { "Total Articles", data.TotalArticles },
                { "Active Articles", data.ActiveArticles },
                { "Draft Articles", data.InactiveArticles },
                { "Total Categories", data.TotalCategories },
                { "Active Categories", data.ActiveCategories },
                { "Total Accounts", data.TotalAccounts },
                { "Staff Accounts", data.StaffAccounts },
                { "Lecturer Accounts", data.LecturerAccounts },
                { "Total Tags", data.TotalTags }
            };

            foreach (var metric in metrics)
            {
                row++;
                worksheet.Cells[row, 1].Value = metric.Key;
                worksheet.Cells[row, 2].Value = metric.Value;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportCategoryReport(List<CategoryStatisticModel> data, DateTime? startDate, DateTime? endDate)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Category Report");

            // Set title
            worksheet.Cells[1, 1].Value = "FU News Management - Articles by Category Report";
            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Set date range
            var dateRange = $"Period: {startDate?.ToString("MMM dd, yyyy") ?? "All Time"} - {endDate?.ToString("MMM dd, yyyy") ?? "Present"}";
            worksheet.Cells[2, 1].Value = dateRange;
            worksheet.Cells[2, 1, 2, 6].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Add generated timestamp
            worksheet.Cells[3, 1].Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}";
            worksheet.Cells[3, 1, 3, 6].Merge = true;
            worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Headers
            var row = 5;
            worksheet.Cells[row, 1].Value = "Category ID";
            worksheet.Cells[row, 2].Value = "Category Name";
            worksheet.Cells[row, 3].Value = "Total Articles";
            worksheet.Cells[row, 4].Value = "Active Articles";
            worksheet.Cells[row, 5].Value = "Draft Articles";
            worksheet.Cells[row, 6].Value = "Percentage (%)";

            // Style headers
            worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;

            // Add data
            foreach (var item in data)
            {
                row++;
                worksheet.Cells[row, 1].Value = item.CategoryId;
                worksheet.Cells[row, 2].Value = item.CategoryName;
                worksheet.Cells[row, 3].Value = item.TotalArticles;
                worksheet.Cells[row, 4].Value = item.ActiveArticles;
                worksheet.Cells[row, 5].Value = item.InactiveArticles;
                worksheet.Cells[row, 6].Value = item.Percentage;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "0.0";
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportAuthorReport(List<AuthorStatisticModel> data, DateTime? startDate, DateTime? endDate)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Author Report");

            // Set title
            worksheet.Cells[1, 1].Value = "FU News Management - Articles by Author Report";
            worksheet.Cells[1, 1, 1, 7].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Set date range
            var dateRange = $"Period: {startDate?.ToString("MMM dd, yyyy") ?? "All Time"} - {endDate?.ToString("MMM dd, yyyy") ?? "Present"}";
            worksheet.Cells[2, 1].Value = dateRange;
            worksheet.Cells[2, 1, 2, 7].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Add generated timestamp
            worksheet.Cells[3, 1].Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}";
            worksheet.Cells[3, 1, 3, 7].Merge = true;
            worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Headers
            var row = 5;
            worksheet.Cells[row, 1].Value = "Author ID";
            worksheet.Cells[row, 2].Value = "Author Name";
            worksheet.Cells[row, 3].Value = "Email";
            worksheet.Cells[row, 4].Value = "Total Articles";
            worksheet.Cells[row, 5].Value = "Active Articles";
            worksheet.Cells[row, 6].Value = "Draft Articles";
            worksheet.Cells[row, 7].Value = "Last Article Date";

            // Style headers
            worksheet.Cells[row, 1, row, 7].Style.Font.Bold = true;

            // Add data
            foreach (var item in data)
            {
                row++;
                worksheet.Cells[row, 1].Value = item.AuthorId;
                worksheet.Cells[row, 2].Value = item.AuthorName;
                worksheet.Cells[row, 3].Value = item.AuthorEmail;
                worksheet.Cells[row, 4].Value = item.TotalArticles;
                worksheet.Cells[row, 5].Value = item.ActiveArticles;
                worksheet.Cells[row, 6].Value = item.InactiveArticles;
                worksheet.Cells[row, 7].Value = item.LastArticleDate;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public byte[] ExportMonthlyReport(List<MonthlyStatistic> data, int year)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Monthly Report");

            // Set title
            worksheet.Cells[1, 1].Value = $"FU News Management - Monthly Statistics Report ({year})";
            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Add generated timestamp
            worksheet.Cells[2, 1].Value = $"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}";
            worksheet.Cells[2, 1, 2, 6].Merge = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Headers
            var row = 4;
            worksheet.Cells[row, 1].Value = "Year";
            worksheet.Cells[row, 2].Value = "Month";
            worksheet.Cells[row, 3].Value = "Month Name";
            worksheet.Cells[row, 4].Value = "Total Articles";
            worksheet.Cells[row, 5].Value = "Active Articles";
            worksheet.Cells[row, 6].Value = "Draft Articles";

            // Style headers
            worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;

            // Add data
            foreach (var item in data)
            {
                row++;
                worksheet.Cells[row, 1].Value = item.Year;
                worksheet.Cells[row, 2].Value = item.Month;
                worksheet.Cells[row, 3].Value = item.MonthName;
                worksheet.Cells[row, 4].Value = item.TotalArticles;
                worksheet.Cells[row, 5].Value = item.ActiveArticles;
                worksheet.Cells[row, 6].Value = item.InactiveArticles;
            }

            // Add summary row
            if (data.Any())
            {
                row += 2;
                worksheet.Cells[row, 1].Value = "TOTAL";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 4].Value = data.Sum(x => x.TotalArticles);
                worksheet.Cells[row, 5].Value = data.Sum(x => x.ActiveArticles);
                worksheet.Cells[row, 6].Value = data.Sum(x => x.InactiveArticles);
                worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }
    }
}