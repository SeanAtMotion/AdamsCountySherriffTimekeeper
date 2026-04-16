namespace Timekeeping.Api.Models.Entities;

public sealed class AuditLog
{
    public long AuditLogId { get; set; }
    public int? ActorEmployeeId { get; set; }
    public Employee? ActorEmployee { get; set; }
    public string ActionType { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
