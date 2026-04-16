namespace Timekeeping.Api.Models.Entities;

public sealed class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleInitial { get; set; }
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Department { get; set; } = "";
    public string? Division { get; set; }
    public string JobTitle { get; set; } = "";
    public string? BadgeNumber { get; set; }
    /// <summary>Immediate supervisor (display name); kept as text to avoid hierarchy complexity.</summary>
    public string? SupervisorName { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public AppRole Role { get; set; } = AppRole.Employee;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<CorrectionRequest> CorrectionRequests { get; set; } = new List<CorrectionRequest>();
}
