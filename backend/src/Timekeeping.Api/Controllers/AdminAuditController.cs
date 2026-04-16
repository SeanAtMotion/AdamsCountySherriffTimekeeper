using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/auditlogs")]
public sealed class AdminAuditController(TimekeepingDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> List([FromQuery] int take = 200, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 1000);
        var rows = await db.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.TimestampUtc)
            .Take(take)
            .Select(a => new AuditLogDto
            {
                AuditLogId = a.AuditLogId,
                ActorEmployeeId = a.ActorEmployeeId,
                ActionType = a.ActionType,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                OldValuesJson = a.OldValuesJson,
                NewValuesJson = a.NewValuesJson,
                TimestampUtc = a.TimestampUtc,
                IpAddress = a.IpAddress
            })
            .ToListAsync(ct);
        return Ok(rows);
    }
}
