using DataAccess.Models;

namespace DataAccess.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<SystemAccount> AccountRepository { get; }
        IRepository<Category> CategoryRepository { get; }
        IRepository<NewsArticle> NewsArticleRepository { get; }
        ITagRepository TagRepository { get; }
        IRepository<AuditLog> AuditLogRepository { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}