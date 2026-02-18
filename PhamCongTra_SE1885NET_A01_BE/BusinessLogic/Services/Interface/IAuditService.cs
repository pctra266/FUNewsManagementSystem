using System.Threading.Tasks;
using DataAccess.DTOs;
using DataAccess.Models;
using System.Linq;

namespace BussinessLogic.Services
{
    public interface IAuditService
    {
        Task LogAsync(short? userId, string action, string entity, string entityId, object? oldVal, object? newVal);
        Task<List<AuditLogDto>> GetAuditLogsAsync(short? userId = null, string? entityType = null);
        IQueryable<AuditLog> GetAuditLogsQueryable();
    }
}
