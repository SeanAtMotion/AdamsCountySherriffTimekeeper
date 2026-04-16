using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Timekeeping.Api.Data;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;
using Timekeeping.Api.Options;

namespace Timekeeping.Api.Services;

public interface ITimeEntryService
{
    Task<TimeEntry?> GetActiveOpenEntryAsync(int employeeId, CancellationToken ct = default);
    Task<ClockSessionState> GetSessionStateAsync(int employeeId, CancellationToken ct = default);
    Task<TimeEntry> ClockInAsync(int employeeId, CancellationToken ct = default);
    Task<TimeEntry> ClockOutAsync(int employeeId, CancellationToken ct = default);
    Task<TimeEntry> BreakStartAsync(int employeeId, CancellationToken ct = default);
    Task<TimeEntry> BreakEndAsync(int employeeId, CancellationToken ct = default);
    Task RecalculateClosedEntryAsync(TimekeepingDbContext db, TimeEntry entry, CancellationToken ct = default);
    Task FlagSuspiciousEntriesAsync(int employeeId, CancellationToken ct = default);
}

public sealed class TimeEntryService(
    TimekeepingDbContext db,
    IOfficeTimeProvider officeTime,
    IOptions<TimekeepingOptions> options) : ITimeEntryService
{
    private readonly TimekeepingOptions _opt = options.Value;

    public async Task<TimeEntry?> GetActiveOpenEntryAsync(int employeeId, CancellationToken ct = default)
    {
        return await db.TimeEntries
            .Where(e => e.EmployeeId == employeeId && e.ClockOutUtc == null)
            .OrderByDescending(e => e.ClockInUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ClockSessionState> GetSessionStateAsync(int employeeId, CancellationToken ct = default)
    {
        var open = await GetActiveOpenEntryAsync(employeeId, ct);
        if (open is null) return ClockSessionState.ClockedOut;
        if (open.BreakStartUtc is not null && open.BreakEndUtc is null)
            return ClockSessionState.OnBreak;
        return ClockSessionState.ClockedIn;
    }

    public async Task<TimeEntry> ClockInAsync(int employeeId, CancellationToken ct = default)
    {
        var existing = await GetActiveOpenEntryAsync(employeeId, ct);
        if (existing is not null)
            throw new InvalidOperationException("You already have an open time entry. Clock out before clocking in again.");

        var now = DateTime.UtcNow;
        var workDate = officeTime.GetWorkDateUtc(now);
        var entry = new TimeEntry
        {
            EmployeeId = employeeId,
            WorkDate = workDate,
            ClockInUtc = now,
            EntryStatus = TimeEntryStatus.Open,
            UpdatedAtUtc = now,
            CreatedAtUtc = now
        };
        db.TimeEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return entry;
    }

    public async Task<TimeEntry> ClockOutAsync(int employeeId, CancellationToken ct = default)
    {
        var open = await GetActiveOpenEntryAsync(employeeId, ct)
                   ?? throw new InvalidOperationException("No open time entry to clock out.");

        if (open.BreakStartUtc is not null && open.BreakEndUtc is null)
            throw new InvalidOperationException("End your break before clocking out.");

        var now = DateTime.UtcNow;
        open.ClockOutUtc = now;
        open.UpdatedAtUtc = now;
        open.EntryStatus = TimeEntryStatus.Closed;

        await RecalculateClosedEntryAsync(db, open, ct);
        await FlagEntryIfSuspiciousAsync(open, ct);
        await db.SaveChangesAsync(ct);
        await FlagSuspiciousEntriesAsync(employeeId, ct);
        return open;
    }

    public async Task<TimeEntry> BreakStartAsync(int employeeId, CancellationToken ct = default)
    {
        if (!_opt.EnableMealBreaks)
            throw new InvalidOperationException("Meal breaks are disabled by configuration.");

        var open = await GetActiveOpenEntryAsync(employeeId, ct)
                   ?? throw new InvalidOperationException("You must be clocked in to start a break.");

        if (open.BreakStartUtc is not null && open.BreakEndUtc is null)
            throw new InvalidOperationException("A break is already in progress.");

        open.BreakStartUtc = DateTime.UtcNow;
        open.BreakEndUtc = null;
        open.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return open;
    }

    public async Task<TimeEntry> BreakEndAsync(int employeeId, CancellationToken ct = default)
    {
        if (!_opt.EnableMealBreaks)
            throw new InvalidOperationException("Meal breaks are disabled by configuration.");

        var open = await GetActiveOpenEntryAsync(employeeId, ct)
                   ?? throw new InvalidOperationException("You must be clocked in to end a break.");

        if (open.BreakStartUtc is null || open.BreakEndUtc is not null)
            throw new InvalidOperationException("No active break to end.");

        open.BreakEndUtc = DateTime.UtcNow;
        open.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return open;
    }

    public async Task RecalculateClosedEntryAsync(TimekeepingDbContext context, TimeEntry entry, CancellationToken ct = default)
    {
        if (entry.ClockOutUtc is null) return;

        var breakMins = TimeEntryCalculation.BreakMinutes(entry.BreakStartUtc, entry.BreakEndUtc);
        entry.TotalBreakMinutes = breakMins;
        var worked = TimeEntryCalculation.GrossWorkedMinutes(entry.ClockInUtc, entry.ClockOutUtc.Value, breakMins);
        entry.TotalMinutesWorked = worked;

        var weekStartDate = TimeEntryCalculation.StartOfWeekMonday(entry.WorkDate);
        var weekEndDate = weekStartDate.AddDays(6);

        var prior = await context.TimeEntries
            .Where(e => e.EmployeeId == entry.EmployeeId
                        && e.TimeEntryId != entry.TimeEntryId
                        && e.ClockOutUtc != null
                        && e.ClockOutUtc < entry.ClockOutUtc
                        && e.WorkDate >= weekStartDate && e.WorkDate <= weekEndDate)
            .OrderBy(e => e.ClockOutUtc)
            .ToListAsync(ct);

        var usedRegular = prior.Sum(e => e.RegularMinutes);
        var (reg, ot) = TimeEntryCalculation.AllocateWeeklyOvertime(worked, usedRegular);
        entry.RegularMinutes = reg;
        entry.OvertimeMinutes = ot;
        entry.UpdatedAtUtc = DateTime.UtcNow;
    }

    private async Task FlagEntryIfSuspiciousAsync(TimeEntry entry, CancellationToken ct)
    {
        if (entry.ClockOutUtc is null) return;

        if (entry.TotalMinutesWorked < 0)
            entry.EntryStatus = TimeEntryStatus.NeedsReview;

        if (entry.BreakStartUtc is not null && entry.BreakEndUtc is not null && entry.BreakEndUtc < entry.BreakStartUtc)
            entry.EntryStatus = TimeEntryStatus.NeedsReview;

        if (entry.ClockOutUtc < entry.ClockInUtc)
            entry.EntryStatus = TimeEntryStatus.NeedsReview;

        var overlap = await db.TimeEntries.AnyAsync(e =>
            e.EmployeeId == entry.EmployeeId
            && e.TimeEntryId != entry.TimeEntryId
            && e.ClockInUtc < entry.ClockOutUtc
            && (e.ClockOutUtc ?? DateTime.MaxValue) > entry.ClockInUtc, ct);

        if (overlap)
            entry.EntryStatus = TimeEntryStatus.NeedsReview;

        await Task.CompletedTask;
    }

    public async Task FlagSuspiciousEntriesAsync(int employeeId, CancellationToken ct)
    {
        var opens = await db.TimeEntries
            .Where(e => e.EmployeeId == employeeId && e.ClockOutUtc == null && e.ClockInUtc < DateTime.UtcNow.AddHours(-24))
            .ToListAsync(ct);
        foreach (var e in opens)
        {
            e.EntryStatus = TimeEntryStatus.NeedsReview;
            e.UpdatedAtUtc = DateTime.UtcNow;
        }
        if (opens.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
