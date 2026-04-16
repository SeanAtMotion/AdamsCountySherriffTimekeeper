using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public interface IAuditService
{
    Task LogAsync(int? actorEmployeeId, string actionType, string entityType, string entityId, object? oldValues, object? newValues, CancellationToken ct = default);
}

public sealed class AuditService(TimekeepingDbContext db, IHttpContextAccessor httpContextAccessor) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public async Task LogAsync(int? actorEmployeeId, string actionType, string entityType, string entityId, object? oldValues, object? newValues, CancellationToken ct = default)
    {
        var ip = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var row = new AuditLog
        {
            ActorEmployeeId = actorEmployeeId,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            OldValuesJson = oldValues is null ? null : JsonSerializer.Serialize(oldValues, JsonOpts),
            NewValuesJson = newValues is null ? null : JsonSerializer.Serialize(newValues, JsonOpts),
            TimestampUtc = DateTime.UtcNow,
            IpAddress = ip
        };
        db.AuditLogs.Add(row);
        await db.SaveChangesAsync(ct);
    }
}
