using Timekeeping.Api.Models;

namespace Timekeeping.Api.DTOs;

public sealed class TimeEntryDto
{
    public int TimeEntryId { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public DateTime? BreakStartUtc { get; set; }
    public DateTime? BreakEndUtc { get; set; }
    public string? Notes { get; set; }
    public TimeEntryStatus EntryStatus { get; set; }
    public int TotalMinutesWorked { get; set; }
    public int TotalBreakMinutes { get; set; }
    public int RegularMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
}

public sealed class EmployeeDashboardDto
{
    public ClockSessionState SessionState { get; set; }
    public TimeEntryDto? ActiveEntry { get; set; }
    public bool BreaksEnabled { get; set; }
    public IReadOnlyList<TimeEntryDto> RecentEntries { get; set; } = Array.Empty<TimeEntryDto>();
}

public sealed class AdminTimeEntryQuery
{
    public int? EmployeeId { get; set; }
    public string? Department { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public TimeEntryStatus? Status { get; set; }
    public bool? MissingClockOut { get; set; }
    public bool? OvertimeCandidates { get; set; }
}

public sealed class AdminUpdateTimeEntryRequest
{
    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public DateTime? BreakStartUtc { get; set; }
    public DateTime? BreakEndUtc { get; set; }
    public string? Notes { get; set; }
    public TimeEntryStatus EntryStatus { get; set; }
}
