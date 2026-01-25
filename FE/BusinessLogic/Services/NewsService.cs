using DataAccess.Models;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class NewsService: INewsService
    {
        private readonly INewsRepository _newsRepo;

        public NewsService(INewsRepository newsRepo)
        {
            _newsRepo = newsRepo;
        }

        public async Task<List<NewsArticle>> GetAllNewsAsync()
        {
            var list = await _newsRepo.GetAllAsync();
            // Logic: Mới nhất lên đầu
            return list.OrderByDescending(n => n.CreatedDate).ToList();
        }

        public async Task<List<NewsArticle>> GetMyNewsAsync(short userId)
        {
            var all = await _newsRepo.GetAllAsync();
            // Logic: Lọc theo người tạo
            return all.Where(n => n.CreatedById == userId).OrderByDescending(n => n.CreatedDate).ToList();
        }

        public async Task<List<NewsArticle>> SearchNewsAsync(string keyword)
        {
            var all = await _newsRepo.GetAllAsync();
            if (string.IsNullOrEmpty(keyword)) return all;

            // Logic: Tìm theo Title hoặc Headline (không phân biệt hoa thường)
            return all.Where(n => (n.NewsTitle != null && n.NewsTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                               || (n.Headline != null && n.Headline.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                      .ToList();
        }

        public async Task<NewsArticle?> GetNewsByIdAsync(string id)
        {
            return await _newsRepo.GetByIdAsync(id);
        }

        public async Task CreateNewsAsync(NewsArticle article, short createdById)
        {
            // LOGIC NGHIỆP VỤ QUAN TRỌNG:
            // 1. Gán ngày tạo là hiện tại
            article.CreatedDate = DateTime.Now;
            article.ModifiedDate = DateTime.Now;

            // 2. Gán người tạo
            article.CreatedById = createdById;
            article.UpdatedById = createdById; // Người tạo cũng là người update lần đầu

            // 3. Mặc định Active
            if (article.NewsStatus == null) article.NewsStatus = true;

            await _newsRepo.CreateAsync(article);
        }

        public async Task UpdateNewsAsync(NewsArticle article, short updatedById)
        {
            // LOGIC KHI UPDATE:
            // 1. Cập nhật ngày sửa đổi
            article.ModifiedDate = DateTime.Now;

            // 2. Cập nhật người sửa
            article.UpdatedById = updatedById;

            // Chú ý: Backend API cần xử lý việc không được đổi CreatedDate/CreatedBy
            // Nhưng Frontend cũng nên giữ nguyên giá trị cũ để gửi xuống cho đúng format

            await _newsRepo.UpdateAsync(article);
        }

        public async Task DeleteNewsAsync(string id)
        {
            await _newsRepo.DeleteAsync(id);
        }

        public async Task<List<NewsStatistic>> GetNewsStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // Có thể thêm logic kiểm tra ở đây (ví dụ: Start > End thì đổi chỗ, hoặc throw exception)
            if (startDate > endDate)
            {
                return new List<NewsStatistic>();
            }

            return await _newsRepo.GetStatisticsAsync(startDate, endDate);
        }
    }
}
