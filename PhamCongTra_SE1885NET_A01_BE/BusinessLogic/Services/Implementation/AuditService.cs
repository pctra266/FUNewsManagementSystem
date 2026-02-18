using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BussinessLogic.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public AuditService(IUnitOfWork unitOfWork, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            };
        }

        public async Task LogAsync(short? userId, string action, string entity, string entityId, object? oldVal, object? newVal)
        {
            try
            {
                string? userName = null;
                string? userEmail = null;
                short? dbUserId = userId;

                if (userId == 0)
                {
                    // Admin account from config
                    dbUserId = null;
                    userName = "Administrator"; // Or from config if exist
                    userEmail = _configuration["AdminAccount:AccountEmail"];
                }
                else if (userId.HasValue)
                {
                    // Existing user in DB
                    var user = await _unitOfWork.AccountRepository.GetByIdAsync(userId.Value);
                    if (user != null)
                    {
                        userName = user.AccountName;
                        userEmail = user.AccountEmail;
                    }
                }

                var log = new AuditLog
                {
                    UserId = dbUserId,
                    UserName = userName,
                    UserEmail = userEmail,
                    Action = action,
                    EntityName = entity,
                    EntityId = entityId,
                    OldValues = oldVal != null ? JsonSerializer.Serialize(oldVal, _jsonOptions) : null,
                    NewValues = newVal != null ? JsonSerializer.Serialize(newVal, _jsonOptions) : null,
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.AuditLogRepository.AddAsync(log);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Fail silently or log to file/console so we don't block the main operation if audit fails
                Console.WriteLine($"Failed to create audit log: {ex.Message}");
            }
        }

        public async Task<List<AuditLogDto>> GetAuditLogsAsync(short? userId = null, string? entityType = null)
        {
            var query = _unitOfWork.AuditLogRepository.Query()
                .Include(a => a.User)
                .AsQueryable();

            // Apply filters
            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query = query.Where(a => a.EntityName == entityType);
            }

            var logs = await query
                .OrderBy(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    LogId = a.LogId,
                    UserName = a.User.AccountName ?? "Unknown",
                    UserEmail = a.User.AccountEmail ?? "Unknown",
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues,
                    Timestamp = a.Timestamp
                })
                .ToListAsync();

            return logs;
        }

        public IQueryable<AuditLog> GetAuditLogsQueryable()
        {
            return _unitOfWork.AuditLogRepository.Query().Include(a => a.User);
        }
    }
}
