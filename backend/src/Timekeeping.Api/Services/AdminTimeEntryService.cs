using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public interface IAdminTimeEntryService
{
    Task<IReadOnlyList<TimeEntryDto>> QueryAsync(AdminTimeEntryQuery query, CancellationToken ct = default);
    Task<TimeEntryDto?> GetAsync(int id, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateAsync(int actorEmployeeId, int id, AdminUpdateTimeEntryRequest req, CancellationToken ct = default);
    Task RecalculateWeekForEmployeeAsync(int employeeId, DateOnly workDate, CancellationToken ct = default);
}

public sealed class AdminTimeEntryService(
    TimekeepingDbContext db,
    ITimeEntryService timeEntryService,
    IOfficeTimeProvider officeTime,
    IAuditService audit) : IAdminTimeEntryService
{
    public async Task<IReadOnlyList<TimeEntryDto>> QueryAsync(AdminTimeEntryQuery query, CancellationToken ct = default)
    {
        var q = db.TimeEntries
            .Include(e => e.Employee)
            .AsQueryable();

        if (query.EmployeeId is not null)
            q = q.Where(e => e.EmployeeId == query.EmployeeId);

        if (!string.IsNullOrWhiteSpace(query.Department))
        {
            var d = query.Department.Trim();
            q = q.Where(e => e.Employee.Department == d);
        }

        if (query.From is not null)
            q = q.Where(e => e.WorkDate >= query.From);
        if (query.To is not null)
            q = q.Where(e => e.WorkDate <= query.To);

        if (query.Status is not null)
            q = q.Where(e => e.EntryStatus == query.Status);

        if (query.MissingClockOut == true)
            q = q.Where(e => e.ClockOutUtc == null);

        if (query.OvertimeCandidates == true)
            q = q.Where(e => e.OvertimeMinutes > 0);

        var list = await q.OrderByDescending(e => e.WorkDate).ThenByDescending(e => e.ClockInUtc).Take(2000).ToListAsync(ct);
        return list.Select(TimeEntryMapper.ToDto).ToList();
    }

    public async Task<TimeEntryDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var e = await db.TimeEntries.AsNoTracking().FirstOrDefaultAsync(x => x.TimeEntryId == id, ct);
        return e is null ? null : TimeEntryMapper.ToDto(e);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int actorEmployeeId, int id, AdminUpdateTimeEntryRequest req, CancellationToken ct = default)
    {
        var entry = await db.TimeEntries.FirstOrDefaultAsync(e => e.TimeEntryId == id, ct);
        if (entry is null) return (false, "Time entry not found.");

        var old = new
        {
            entry.ClockInUtc,
            entry.ClockOutUtc,
            entry.BreakStartUtc,
            entry.BreakEndUtc,
            entry.Notes,
            entry.EntryStatus
        };

        entry.ClockInUtc = req.ClockInUtc;
        entry.WorkDate = officeTime.GetWorkDateUtc(req.ClockInUtc);
        entry.ClockOutUtc = req.ClockOutUtc;
        entry.BreakStartUtc = req.BreakStartUtc;
        entry.BreakEndUtc = req.BreakEndUtc;
        entry.Notes = req.Notes;
        entry.EntryStatus = req.EntryStatus == TimeEntryStatus.Open && req.ClockOutUtc is null
            ? TimeEntryStatus.Open
            : req.EntryStatus;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        if (entry.ClockOutUtc is not null)
        {
            await timeEntryService.RecalculateClosedEntryAsync(db, entry, ct);
            entry.EntryStatus = req.EntryStatus == TimeEntryStatus.Corrected ? TimeEntryStatus.Corrected : entry.EntryStatus;
        }
        else
        {
            entry.TotalMinutesWorked = 0;
            entry.TotalBreakMinutes = 0;
            entry.RegularMinutes = 0;
            entry.OvertimeMinutes = 0;
        }

        await db.SaveChangesAsync(ct);
        await RecalculateWeekForEmployeeAsync(entry.EmployeeId, entry.WorkDate, ct);

        await audit.LogAsync(actorEmployeeId, "TimeEntry.Update", nameof(TimeEntry), id.ToString(), old, new
        {
            entry.ClockInUtc,
            entry.ClockOutUtc,
            entry.BreakStartUtc,
            entry.BreakEndUtc,
            entry.Notes,
            entry.EntryStatus
        }, ct);

        return (true, null);
    }

    public async Task RecalculateWeekForEmployeeAsync(int employeeId, DateOnly workDate, CancellationToken ct = default)
    {
        var start = TimeEntryCalculation.StartOfWeekMonday(workDate);
        var end = start.AddDays(6);

        var entries = await db.TimeEntries
            .Where(e => e.EmployeeId == employeeId && e.WorkDate >= start && e.WorkDate <= end && e.ClockOutUtc != null)
            .OrderBy(e => e.ClockOutUtc)
            .ToListAsync(ct);

        foreach (var e in entries)
            await timeEntryService.RecalculateClosedEntryAsync(db, e, ct);

        await db.SaveChangesAsync(ct);
    }
}
