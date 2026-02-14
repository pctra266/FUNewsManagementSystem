using Microsoft.EntityFrameworkCore.Storage;
using DataAccess.Models;
using DataAccess.Data;

namespace DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NewsContext _context;
        private IDbContextTransaction? _transaction;
        
        private IRepository<SystemAccount>? _accountRepository;
        private IRepository<Category>? _categoryRepository;

        private IRepository<NewsArticle>? _newsArticleRepository;
        private ITagRepository? _tagRepository;
        private IRepository<AuditLog>? _auditLogRepository;
        private IRepository<NewsArticleImage>? _newsArticleImageRepository;

        public UnitOfWork(NewsContext context)
        {
            _context = context;
        }

        public IRepository<SystemAccount> AccountRepository
        {
            get { return _accountRepository ??= new Repository<SystemAccount>(_context); }
        }

        public IRepository<Category> CategoryRepository
        {
            get { return _categoryRepository ??= new Repository<Category>(_context); }
        }

        public IRepository<NewsArticle> NewsArticleRepository
        {
            get { return _newsArticleRepository ??= new Repository<NewsArticle>(_context); }
        }

        public ITagRepository TagRepository
        {
            get { return _tagRepository ??= new TagRepository(_context); }
        }

        public IRepository<AuditLog> AuditLogRepository
        {
            get { return _auditLogRepository ??= new Repository<AuditLog>(_context); }
        }

        public IRepository<NewsArticleImage> NewsArticleImageRepository
        {
            get { return _newsArticleImageRepository ??= new Repository<NewsArticleImage>(_context); }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}