namespace Timekeeping.Api.Options;

public sealed class TimekeepingOptions
{
    public const string SectionName = "Timekeeping";

    /// <summary>IANA time zone id for display and business-date calculations (e.g. America/New_York).</summary>
    public string OfficeTimeZoneId { get; set; } = "America/New_York";

    /// <summary>Regular hours per week before overtime applies.</summary>
    public double OvertimeWeeklyThresholdHours { get; set; } = 40;

    /// <summary>When false, break endpoints return 400 and UI should hide break controls.</summary>
    public bool EnableMealBreaks { get; set; } = true;
}
