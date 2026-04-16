namespace Timekeeping.Api.Models.Entities;

public sealed class CorrectionRequest
{
    public int CorrectionRequestId { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int TimeEntryId { get; set; }
    public TimeEntry TimeEntry { get; set; } = null!;

    public DateTime? RequestedClockInUtc { get; set; }
    public DateTime? RequestedClockOutUtc { get; set; }
    public DateTime? RequestedBreakStartUtc { get; set; }
    public DateTime? RequestedBreakEndUtc { get; set; }
    public string Reason { get; set; } = "";
    public CorrectionRequestStatus Status { get; set; } = CorrectionRequestStatus.Pending;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
    public int? ReviewedByEmployeeId { get; set; }
    public Employee? ReviewedByEmployee { get; set; }
    public string? ReviewNotes { get; set; }
}
