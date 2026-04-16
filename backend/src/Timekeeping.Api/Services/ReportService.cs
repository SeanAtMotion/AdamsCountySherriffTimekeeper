using System.Text;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;

namespace Timekeeping.Api.Services;

public interface IReportService
{
    Task<IReadOnlyList<HoursSummaryRowDto>> HoursSummaryAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<OvertimeRowDto>> OvertimeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<MissingPunchRowDto>> MissingPunchesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceSummaryRowDto>> AttendanceAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<byte[]> ExportCsvAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}

public sealed class ReportService(TimekeepingDbContext db) : IReportService
{
    public async Task<IReadOnlyList<HoursSummaryRowDto>> HoursSummaryAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var raw = await db.TimeEntries
            .AsNoTracking()
            .Include(e => e.Employee)
            .Where(e => e.WorkDate >= from && e.WorkDate <= to && e.ClockOutUtc != null)
            .Select(e => new
            {
                e.EmployeeId,
                e.Employee.EmployeeNumber,
                e.Employee.FirstName,
                e.Employee.LastName,
                e.Employee.Department,
                e.RegularMinutes,
                e.OvertimeMinutes,
                e.TotalMinutesWorked
            })
            .ToListAsync(ct);

        return raw
            .GroupBy(x => new { x.EmployeeId, x.EmployeeNumber, x.FirstName, x.LastName, x.Department })
            .Select(g => new HoursSummaryRowDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeNumber = g.Key.EmployeeNumber,
                FullName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                Department = g.Key.Department,
                RegularHours = g.Sum(x => x.RegularMinutes) / 60.0,
                OvertimeHours = g.Sum(x => x.OvertimeMinutes) / 60.0,
                TotalHours = g.Sum(x => x.TotalMinutesWorked) / 60.0
            })
            .OrderBy(r => r.FullName)
            .ToList();
    }

    public async Task<IReadOnlyList<OvertimeRowDto>> OvertimeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var raw = await db.TimeEntries
            .AsNoTracking()
            .Include(e => e.Employee)
            .Where(e => e.WorkDate >= from && e.WorkDate <= to && e.ClockOutUtc != null && e.OvertimeMinutes > 0)
            .Select(e => new
            {
                e.EmployeeId,
                e.Employee.FirstName,
                e.Employee.LastName,
                e.Employee.Department,
                e.OvertimeMinutes
            })
            .ToListAsync(ct);

        return raw
            .GroupBy(x => new { x.EmployeeId, x.FirstName, x.LastName, x.Department })
            .Select(g => new OvertimeRowDto
            {
                EmployeeId = g.Key.EmployeeId,
                FullName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                Department = g.Key.Department,
                OvertimeHours = g.Sum(x => x.OvertimeMinutes) / 60.0
            })
            .OrderByDescending(r => r.OvertimeHours)
            .ToList();
    }

    public async Task<IReadOnlyList<MissingPunchRowDto>> MissingPunchesAsync(CancellationToken ct = default)
    {
        var open = await db.TimeEntries
            .AsNoTracking()
            .Include(e => e.Employee)
            .Where(e => e.ClockOutUtc == null)
            .OrderByDescending(e => e.ClockInUtc)
            .Take(500)
            .ToListAsync(ct);

        var list = new List<MissingPunchRowDto>();
        foreach (var e in open)
        {
            list.Add(new MissingPunchRowDto
            {
                TimeEntryId = e.TimeEntryId,
                EmployeeId = e.EmployeeId,
                FullName = e.Employee.FirstName + " " + e.Employee.LastName,
                Department = e.Employee.Department,
                WorkDate = e.WorkDate,
                Issue = "Missing clock-out / open entry"
            });
        }

        var review = await db.TimeEntries
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => x.EntryStatus == TimeEntryStatus.NeedsReview && x.ClockOutUtc != null)
            .OrderByDescending(x => x.WorkDate)
            .Take(200)
            .ToListAsync(ct);

        foreach (var e in review)
        {
            list.Add(new MissingPunchRowDto
            {
                TimeEntryId = e.TimeEntryId,
                EmployeeId = e.EmployeeId,
                FullName = e.Employee.FirstName + " " + e.Employee.LastName,
                Department = e.Employee.Department,
                WorkDate = e.WorkDate,
                Issue = "Flagged for review"
            });
        }

        return list;
    }

    public async Task<IReadOnlyList<AttendanceSummaryRowDto>> AttendanceAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        return await db.TimeEntries
            .AsNoTracking()
            .Where(e => e.WorkDate >= from && e.WorkDate <= to && e.ClockOutUtc != null)
            .GroupBy(e => e.WorkDate)
            .Select(g => new AttendanceSummaryRowDto
            {
                Date = g.Key,
                Headcount = g.Select(x => x.EmployeeId).Distinct().Count(),
                TotalHours = g.Sum(x => x.TotalMinutesWorked) / 60.0
            })
            .OrderBy(r => r.Date)
            .ToListAsync(ct);
    }

    public async Task<byte[]> ExportCsvAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var summary = await HoursSummaryAsync(from, to, ct);
        var sb = new StringBuilder();
        sb.AppendLine("EmployeeId,EmployeeNumber,FullName,Department,RegularHours,OvertimeHours,TotalHours");
        foreach (var r in summary)
        {
            sb.AppendLine(string.Join(',',
                r.EmployeeId,
                Escape(r.EmployeeNumber),
                Escape(r.FullName),
                Escape(r.Department),
                r.RegularHours.ToString("F2"),
                r.OvertimeHours.ToString("F2"),
                r.TotalHours.ToString("F2")));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
}
