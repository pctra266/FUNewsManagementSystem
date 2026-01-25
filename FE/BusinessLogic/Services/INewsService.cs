using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public interface INewsService
    {
        Task<List<NewsArticle>> GetAllNewsAsync();
        Task<List<NewsArticle>> GetMyNewsAsync(short userId); // Xem tin của chính mình tạo
        Task<List<NewsArticle>> SearchNewsAsync(string keyword);
        Task<NewsArticle?> GetNewsByIdAsync(string id);
        Task CreateNewsAsync(NewsArticle article, short createdById);
        Task UpdateNewsAsync(NewsArticle article, short updatedById);
        Task DeleteNewsAsync(string id);

        Task<List<NewsStatistic>> GetNewsStatisticsAsync(DateTime startDate, DateTime endDate);
    }
}
