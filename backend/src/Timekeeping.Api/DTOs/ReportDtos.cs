namespace Timekeeping.Api.DTOs;

public sealed class HoursSummaryRowDto
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public double RegularHours { get; set; }
    public double OvertimeHours { get; set; }
    public double TotalHours { get; set; }
}

public sealed class OvertimeRowDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public double OvertimeHours { get; set; }
}

public sealed class MissingPunchRowDto
{
    public int TimeEntryId { get; set; }
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public DateOnly WorkDate { get; set; }
    public string Issue { get; set; } = "";
}

public sealed class AttendanceSummaryRowDto
{
    public DateOnly Date { get; set; }
    public int Headcount { get; set; }
    public double TotalHours { get; set; }
}

public sealed class AdminDashboardStatsDto
{
    public int ActiveEmployees { get; set; }
    public int ClockedInNow { get; set; }
    public int OpenMissingPunches { get; set; }
    public int OvertimeCandidates { get; set; }
    public int PendingCorrections { get; set; }
}

public sealed class AuditLogDto
{
    public long AuditLogId { get; set; }
    public int? ActorEmployeeId { get; set; }
    public string ActionType { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? IpAddress { get; set; }
}
