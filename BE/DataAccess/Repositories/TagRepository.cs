using DataAccess.Models;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly NewsContext _context;

        public TagRepository(NewsContext context)
        {
            _context = context;
        }

        public void AddTag(Tag tag)
        {
            _context.Tags.Add(tag);
            _context.SaveChanges();
        }

        public void DeleteTag(int id)
        {
            // Cần load cả quan hệ NewsArticles để xóa sạch liên kết trong bảng trung gian trước
            var tag = _context.Tags
                .Include(t => t.NewsArticles)
                .FirstOrDefault(t => t.TagId == id);

            if (tag != null)
            {
                // Xóa liên kết trong bảng junction (Many-to-Many)
                tag.NewsArticles.Clear();

                // Xóa Tag
                _context.Tags.Remove(tag);
                _context.SaveChanges();
            }
        }

        public List<Tag> GetTags()
        {
            return _context.Tags
                .Include(t => t.NewsArticles)
                .ToList();
        }

        public Tag GetTagById(int id)
        {
            return _context.Tags
                .Include(t => t.NewsArticles)
                .FirstOrDefault(t => t.TagId == id);
        }

        // Tối ưu hóa tìm kiếm: Filter ngay tại Database thay vì lấy hết về rồi mới Filter
        public List<Tag> Search(string keyword)
        {
            return _context.Tags
                .Where(t => t.TagName.Contains(keyword))
                .Include(t => t.NewsArticles)
                .ToList();
        }

        public void UpdateTag(Tag tag)
        {
            // Logic Update phức tạp cần xử lý quan hệ Many-to-Many
            var existingTag = _context.Tags
                .Include(t => t.NewsArticles)
                .FirstOrDefault(t => t.TagId == tag.TagId);

            if (existingTag != null)
            {
                // 1. Cập nhật thông tin cơ bản
                existingTag.TagName = tag.TagName;
                existingTag.Note = tag.Note;

                // 2. Xử lý cập nhật danh sách bài viết liên quan (Logic của DAO cũ)
                // Xóa các liên kết cũ
                existingTag.NewsArticles.Clear();

                // Thêm các liên kết mới (nếu có truyền vào)
                if (tag.NewsArticles != null && tag.NewsArticles.Any())
                {
                    foreach (var article in tag.NewsArticles)
                    {
                        // Đảm bảo article tồn tại trong DB trước khi attach lại
                        var dbArticle = _context.NewsArticles.Find(article.NewsArticleId);
                        if (dbArticle != null)
                        {
                            existingTag.NewsArticles.Add(dbArticle);
                        }
                    }
                }

                _context.SaveChanges();
            }
        }
    }
}