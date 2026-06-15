using DemoApp.Web.Data;
using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Web.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.PerformedAt)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs.FindAsync(id);
        }

        public async Task<List<AuditLog>> GetByEntityAsync(string entityName)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityName == entityName)
                .OrderByDescending(a => a.PerformedAt)
                .ToListAsync();
        }

        public async Task<int> PurgeAllAsync()
        {
            return await _context.AuditLogs.ExecuteDeleteAsync();
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> GetFilteredAsync(AuditLogFilterDto filter)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.PerformedBy))
                query = query.Where(a => a.PerformedBy.Contains(filter.PerformedBy));

            if (!string.IsNullOrWhiteSpace(filter.EntityName))
                query = query.Where(a => a.EntityName == filter.EntityName);

            if (!string.IsNullOrWhiteSpace(filter.OperationType))
                query = query.Where(a => a.OperationType == filter.OperationType);

            if (filter.PerformedFrom.HasValue)
                query = query.Where(a => a.PerformedAt >= filter.PerformedFrom.Value);

            if (filter.PerformedTo.HasValue)
                query = query.Where(a => a.PerformedAt <= filter.PerformedTo.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(a => a.PerformedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (logs, totalCount);
        }

    }

}
