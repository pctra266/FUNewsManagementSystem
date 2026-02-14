using DataAccess.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BussinessLogic.Services
{
    public interface INewsArticleImageService
    {
        Task<IEnumerable<NewsArticleImageDto>> GetImagesByArticleIdAsync(string articleId);
        Task<NewsArticleImageDto?> GetImageByIdAsync(int id);
        Task<NewsArticleImageDto> AddImageAsync(string articleId, NewsArticleImageCreateDto dto);
        Task<NewsArticleImageDto> UpdateImageAsync(int id, NewsArticleImageUpdateDto dto);
        Task<bool> DeleteImageAsync(int id);
    }
}
