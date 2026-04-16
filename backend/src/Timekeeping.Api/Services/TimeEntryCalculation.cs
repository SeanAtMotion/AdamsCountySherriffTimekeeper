using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public static class TimeEntryCalculation
{
    public static int BreakMinutes(DateTime? breakStartUtc, DateTime? breakEndUtc)
    {
        if (breakStartUtc is null || breakEndUtc is null) return 0;
        if (breakEndUtc < breakStartUtc) return 0;
        return (int)Math.Round((breakEndUtc.Value - breakStartUtc.Value).TotalMinutes);
    }

    public static int GrossWorkedMinutes(DateTime clockInUtc, DateTime clockOutUtc, int breakMinutes)
    {
        if (clockOutUtc < clockInUtc) return 0;
        var gross = (int)Math.Round((clockOutUtc - clockInUtc).TotalMinutes);
        return Math.Max(0, gross - breakMinutes);
    }

    /// <summary>
    /// Allocates regular vs overtime minutes for this entry within its ISO week (Monday start) using FIFO ordering.
    /// </summary>
    public static (int Regular, int Overtime) AllocateWeeklyOvertime(
        int entryWorkedMinutes,
        int weekRegularUsedBeforeThisEntry)
    {
        var threshold = 40 * 60;
        var remainingRegular = Math.Max(0, threshold - weekRegularUsedBeforeThisEntry);
        var regular = Math.Min(entryWorkedMinutes, remainingRegular);
        var overtime = entryWorkedMinutes - regular;
        return (regular, overtime);
    }

    /// <summary>Monday-based work week in the office calendar.</summary>
    public static DateOnly StartOfWeekMonday(DateOnly date)
    {
        var dow = date.DayOfWeek;
        var offset = dow == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - dow;
        return date.AddDays((int)offset);
    }
}
