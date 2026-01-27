using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class DashboardStatisticsModel
    {
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
        public int TotalCategories { get; set; }
        public int ActiveCategories { get; set; }
        public int TotalAccounts { get; set; }
        public int StaffAccounts { get; set; }
        public int LecturerAccounts { get; set; }
        public int TotalTags { get; set; }
        public List<RecentArticleModel> RecentArticles { get; set; } = new List<RecentArticleModel>();
        public MonthlyStatsModel? MonthlyStats { get; set; }
    }
}
