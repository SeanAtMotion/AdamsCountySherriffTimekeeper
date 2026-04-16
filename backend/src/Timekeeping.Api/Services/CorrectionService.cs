using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public interface ICorrectionService
{
    Task<(bool Success, string? Error)> CreateAsync(int employeeId, CreateCorrectionRequestDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<CorrectionRequestDto>> MyRequestsAsync(int employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<CorrectionRequestDto>> AdminListAsync(CorrectionRequestStatus? status, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ApproveAsync(int adminEmployeeId, int id, ReviewCorrectionRequest review, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DenyAsync(int adminEmployeeId, int id, ReviewCorrectionRequest review, CancellationToken ct = default);
}

public sealed class CorrectionService(
    TimekeepingDbContext db,
    IAdminTimeEntryService adminTime,
    ITimeEntryService timeEntryService,
    IOfficeTimeProvider officeTime,
    IAuditService audit) : ICorrectionService
{
    public async Task<(bool Success, string? Error)> CreateAsync(int employeeId, CreateCorrectionRequestDto dto, CancellationToken ct = default)
    {
        var entry = await db.TimeEntries.FirstOrDefaultAsync(e => e.TimeEntryId == dto.TimeEntryId && e.EmployeeId == employeeId, ct);
        if (entry is null) return (false, "Time entry not found.");

        var pending = await db.CorrectionRequests.AnyAsync(c =>
            c.TimeEntryId == dto.TimeEntryId && c.Status == CorrectionRequestStatus.Pending, ct);
        if (pending) return (false, "A pending correction already exists for this entry.");

        var row = new CorrectionRequest
        {
            EmployeeId = employeeId,
            TimeEntryId = dto.TimeEntryId,
            RequestedClockInUtc = dto.RequestedClockInUtc,
            RequestedClockOutUtc = dto.RequestedClockOutUtc,
            RequestedBreakStartUtc = dto.RequestedBreakStartUtc,
            RequestedBreakEndUtc = dto.RequestedBreakEndUtc,
            Reason = dto.Reason.Trim(),
            Status = CorrectionRequestStatus.Pending,
            SubmittedAtUtc = DateTime.UtcNow
        };
        db.CorrectionRequests.Add(row);
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<IReadOnlyList<CorrectionRequestDto>> MyRequestsAsync(int employeeId, CancellationToken ct = default)
    {
        var list = await db.CorrectionRequests
            .AsNoTracking()
            .Include(c => c.TimeEntry)
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.SubmittedAtUtc)
            .Take(200)
            .ToListAsync(ct);

        return list.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<CorrectionRequestDto>> AdminListAsync(CorrectionRequestStatus? status, CancellationToken ct = default)
    {
        var q = db.CorrectionRequests.AsNoTracking().Include(c => c.TimeEntry).Include(c => c.Employee).AsQueryable();
        if (status is not null)
            q = q.Where(c => c.Status == status);
        var list = await q.OrderByDescending(c => c.SubmittedAtUtc).Take(500).ToListAsync(ct);
        return list.Select(Map).ToList();
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int adminEmployeeId, int id, ReviewCorrectionRequest review, CancellationToken ct = default)
    {
        var req = await db.CorrectionRequests.Include(c => c.TimeEntry).FirstOrDefaultAsync(c => c.CorrectionRequestId == id, ct);
        if (req is null) return (false, "Request not found.");
        if (req.Status != CorrectionRequestStatus.Pending) return (false, "Request is not pending.");

        var entry = req.TimeEntry;
        var old = TimeEntryMapper.ToDto(entry);

        if (req.RequestedClockInUtc is not null)
        {
            entry.ClockInUtc = req.RequestedClockInUtc.Value;
            entry.WorkDate = officeTime.GetWorkDateUtc(entry.ClockInUtc);
        }
        if (req.RequestedClockOutUtc is not null) entry.ClockOutUtc = req.RequestedClockOutUtc;
        if (req.RequestedBreakStartUtc is not null) entry.BreakStartUtc = req.RequestedBreakStartUtc;
        if (req.RequestedBreakEndUtc is not null) entry.BreakEndUtc = req.RequestedBreakEndUtc;
        entry.EntryStatus = TimeEntryStatus.Corrected;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        if (entry.ClockOutUtc is not null)
            await timeEntryService.RecalculateClosedEntryAsync(db, entry, ct);

        req.Status = CorrectionRequestStatus.Approved;
        req.ReviewedAtUtc = DateTime.UtcNow;
        req.ReviewedByEmployeeId = adminEmployeeId;
        req.ReviewNotes = review.ReviewNotes;

        await db.SaveChangesAsync(ct);
        await adminTime.RecalculateWeekForEmployeeAsync(entry.EmployeeId, entry.WorkDate, ct);

        await audit.LogAsync(adminEmployeeId, "Correction.Approve", nameof(TimeEntry), entry.TimeEntryId.ToString(), old, TimeEntryMapper.ToDto(entry), ct);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DenyAsync(int adminEmployeeId, int id, ReviewCorrectionRequest review, CancellationToken ct = default)
    {
        var req = await db.CorrectionRequests.FirstOrDefaultAsync(c => c.CorrectionRequestId == id, ct);
        if (req is null) return (false, "Request not found.");
        if (req.Status != CorrectionRequestStatus.Pending) return (false, "Request is not pending.");

        req.Status = CorrectionRequestStatus.Denied;
        req.ReviewedAtUtc = DateTime.UtcNow;
        req.ReviewedByEmployeeId = adminEmployeeId;
        req.ReviewNotes = review.ReviewNotes;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    private static CorrectionRequestDto Map(CorrectionRequest c) => new()
    {
        CorrectionRequestId = c.CorrectionRequestId,
        EmployeeId = c.EmployeeId,
        TimeEntryId = c.TimeEntryId,
        RequestedClockInUtc = c.RequestedClockInUtc,
        RequestedClockOutUtc = c.RequestedClockOutUtc,
        RequestedBreakStartUtc = c.RequestedBreakStartUtc,
        RequestedBreakEndUtc = c.RequestedBreakEndUtc,
        Reason = c.Reason,
        Status = c.Status,
        SubmittedAtUtc = c.SubmittedAtUtc,
        ReviewedAtUtc = c.ReviewedAtUtc,
        ReviewedByEmployeeId = c.ReviewedByEmployeeId,
        ReviewNotes = c.ReviewNotes,
        OriginalEntry = TimeEntryMapper.ToDto(c.TimeEntry)
    };
}
