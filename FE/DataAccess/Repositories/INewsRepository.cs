using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public interface INewsRepository
    {
        Task<List<NewsArticle>> GetAllAsync();
        Task<NewsArticle?> GetByIdAsync(string id); 
        Task CreateAsync(NewsArticle article);
        Task UpdateAsync(NewsArticle article);
        Task DeleteAsync(string id);
        Task<List<NewsStatistic>> GetStatisticsAsync(DateTime startDate, DateTime endDate);
    }
}
