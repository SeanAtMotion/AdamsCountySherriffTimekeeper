using Timekeeping.Api.Models;

namespace Timekeeping.Api.DTOs;

public sealed class CreateCorrectionRequestDto
{
    public int TimeEntryId { get; set; }
    public DateTime? RequestedClockInUtc { get; set; }
    public DateTime? RequestedClockOutUtc { get; set; }
    public DateTime? RequestedBreakStartUtc { get; set; }
    public DateTime? RequestedBreakEndUtc { get; set; }
    public string Reason { get; set; } = "";
}

public sealed class CorrectionRequestDto
{
    public int CorrectionRequestId { get; set; }
    public int EmployeeId { get; set; }
    public int TimeEntryId { get; set; }
    public DateTime? RequestedClockInUtc { get; set; }
    public DateTime? RequestedClockOutUtc { get; set; }
    public DateTime? RequestedBreakStartUtc { get; set; }
    public DateTime? RequestedBreakEndUtc { get; set; }
    public string Reason { get; set; } = "";
    public CorrectionRequestStatus Status { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public int? ReviewedByEmployeeId { get; set; }
    public string? ReviewNotes { get; set; }
    public TimeEntryDto? OriginalEntry { get; set; }
}

public sealed class ReviewCorrectionRequest
{
    public string? ReviewNotes { get; set; }
}
