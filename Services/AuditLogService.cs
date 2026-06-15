using DemoApp.Web.Models;
using DemoApp.Web.Models.DTOs;
using DemoApp.Web.Repositories.Interfaces;
using DemoApp.Web.Services.Interfaces;
using DemoApp.Web.Services.Utils;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _repository;

    public AuditLogService(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AuditLog>>> GetAllAsync()
    {
        try
        {
            var logs = await _repository.GetAllAsync();
            return Result<List<AuditLog>>.Success(logs);
        }
        catch (Exception)
        {
            return Result<List<AuditLog>>.Failure("Error al obtener los logs de auditoría.");
        }
    }

    public async Task<Result<AuditLog?>> GetByIdAsync(int id)
    {
        try
        {
            var log = await _repository.GetByIdAsync(id);
            return Result<AuditLog?>.Success(log);
        }
        catch (Exception)
        {
            return Result<AuditLog?>.Failure("Error al obtener el log de auditoría.");
        }
    }

    public async Task<Result<int>> PurgeAllAsync()
    {
        try
        {
            var deleted = await _repository.PurgeAllAsync();
            return Result<int>.Success(deleted);
        }
        catch (Exception)
        {
            return Result<int>.Failure("Error al purgar los logs de auditoría.");
        }
    }

    public async Task<Result<PagedResult<AuditLog>>> GetFilteredAsync(AuditLogFilterDto filter)
    {
        try
        {
            var (logs, total) = await _repository.GetFilteredAsync(filter);
            return Result<PagedResult<AuditLog>>.Success(new PagedResult<AuditLog>
            {
                Items = logs,
                TotalCount = total,
                Page = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }
        catch (Exception)
        {
            return Result<PagedResult<AuditLog>>.Failure("Error al filtrar los logs de auditoría.");
        }
    }
}
