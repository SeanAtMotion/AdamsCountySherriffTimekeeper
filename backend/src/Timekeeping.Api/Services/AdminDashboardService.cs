using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;

namespace Timekeeping.Api.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}

public sealed class AdminDashboardService(TimekeepingDbContext db) : IAdminDashboardService
{
    public async Task<AdminDashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var active = await db.Employees.CountAsync(e => e.IsActive, ct);
        var clockedIn = await db.TimeEntries.CountAsync(e => e.ClockOutUtc == null, ct);
        var missing = await db.TimeEntries.CountAsync(e => e.ClockOutUtc == null, ct);
        var ot = await db.TimeEntries.CountAsync(e => e.ClockOutUtc != null && e.OvertimeMinutes > 0 && e.WorkDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-14)), ct);
        var pending = await db.CorrectionRequests.CountAsync(c => c.Status == CorrectionRequestStatus.Pending, ct);

        return new AdminDashboardStatsDto
        {
            ActiveEmployees = active,
            ClockedInNow = clockedIn,
            OpenMissingPunches = missing,
            OvertimeCandidates = ot,
            PendingCorrections = pending
        };
    }
}
