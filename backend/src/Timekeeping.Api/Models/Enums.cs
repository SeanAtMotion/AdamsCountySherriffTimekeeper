namespace Timekeeping.Api.Models;

public enum AppRole
{
    Employee = 0,
    Admin = 1
}

public enum TimeEntryStatus
{
    Open = 0,
    Closed = 1,
    NeedsReview = 2,
    Corrected = 3
}

public enum CorrectionRequestStatus
{
    Pending = 0,
    Approved = 1,
    Denied = 2
}

/// <summary>Derived clock state for dashboards (not persisted).</summary>
public enum ClockSessionState
{
    ClockedOut = 0,
    ClockedIn = 1,
    OnBreak = 2
}
