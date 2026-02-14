using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.DTOs;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BussinessLogic.Services
{
    public class NewsArticleImageService : INewsArticleImageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NewsArticleImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<NewsArticleImageDto>> GetImagesByArticleIdAsync(string articleId)
        {
            var images = await _unitOfWork.NewsArticleImageRepository.Query()
                .Where(i => i.NewsArticleId == articleId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            return images.Select(i => new NewsArticleImageDto
            {
                ImageId = i.ImageId,
                NewsArticleId = i.NewsArticleId,
                ImageUrl = i.ImageUrl,
                Caption = i.Caption,
                CreatedDate = i.CreatedDate
            });
        }

        public async Task<NewsArticleImageDto?> GetImageByIdAsync(int id)
        {
            var image = await _unitOfWork.NewsArticleImageRepository.Query()
                .FirstOrDefaultAsync(i => i.ImageId == id);

            if (image == null) return null;

            return new NewsArticleImageDto
            {
                ImageId = image.ImageId,
                NewsArticleId = image.NewsArticleId,
                ImageUrl = image.ImageUrl,
                Caption = image.Caption,
                CreatedDate = image.CreatedDate
            };
        }

        public async Task<NewsArticleImageDto> AddImageAsync(string articleId, NewsArticleImageCreateDto dto)
        {
            var image = new NewsArticleImage
            {
                NewsArticleId = articleId,
                ImageUrl = dto.ImageUrl,
                Caption = dto.Caption,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.NewsArticleImageRepository.AddAsync(image);
            await _unitOfWork.SaveChangesAsync();

            return new NewsArticleImageDto
            {
                ImageId = image.ImageId,
                NewsArticleId = image.NewsArticleId,
                ImageUrl = image.ImageUrl,
                Caption = image.Caption,
                CreatedDate = image.CreatedDate
            };
        }

        public async Task<NewsArticleImageDto> UpdateImageAsync(int id, NewsArticleImageUpdateDto dto)
        {
            var image = await _unitOfWork.NewsArticleImageRepository.Query()
                .FirstOrDefaultAsync(i => i.ImageId == id);

            if (image == null) throw new InvalidOperationException("Image not found");

            image.ImageUrl = dto.ImageUrl;
            image.Caption = dto.Caption;
            // image.CreatedDate is not updated as it's creation time

            _unitOfWork.NewsArticleImageRepository.Update(image);
            await _unitOfWork.SaveChangesAsync();

            return new NewsArticleImageDto
            {
                ImageId = image.ImageId,
                NewsArticleId = image.NewsArticleId,
                ImageUrl = image.ImageUrl,
                Caption = image.Caption,
                CreatedDate = image.CreatedDate
            };
        }

        public async Task<bool> DeleteImageAsync(int id)
        {
            var image = await _unitOfWork.NewsArticleImageRepository.Query()
                .FirstOrDefaultAsync(i => i.ImageId == id);

            if (image == null) return false;

            _unitOfWork.NewsArticleImageRepository.Delete(image);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
