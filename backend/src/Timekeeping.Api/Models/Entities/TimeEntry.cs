namespace Timekeeping.Api.Models.Entities;

public sealed class TimeEntry
{
    public int TimeEntryId { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>Calendar date in the office time zone for this shift (business date).</summary>
    public DateOnly WorkDate { get; set; }

    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
    public DateTime? BreakStartUtc { get; set; }
    public DateTime? BreakEndUtc { get; set; }
    public string? Notes { get; set; }
    public TimeEntryStatus EntryStatus { get; set; } = TimeEntryStatus.Open;
    public int TotalMinutesWorked { get; set; }
    public int TotalBreakMinutes { get; set; }
    public int RegularMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CorrectionRequest> CorrectionRequests { get; set; } = new List<CorrectionRequest>();
}
