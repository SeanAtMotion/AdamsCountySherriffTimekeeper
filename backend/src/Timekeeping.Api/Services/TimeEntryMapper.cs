using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public static class TimeEntryMapper
{
    public static TimeEntryDto ToDto(TimeEntry e) => new()
    {
        TimeEntryId = e.TimeEntryId,
        EmployeeId = e.EmployeeId,
        WorkDate = e.WorkDate,
        ClockInUtc = e.ClockInUtc,
        ClockOutUtc = e.ClockOutUtc,
        BreakStartUtc = e.BreakStartUtc,
        BreakEndUtc = e.BreakEndUtc,
        Notes = e.Notes,
        EntryStatus = e.EntryStatus,
        TotalMinutesWorked = e.TotalMinutesWorked,
        TotalBreakMinutes = e.TotalBreakMinutes,
        RegularMinutes = e.RegularMinutes,
        OvertimeMinutes = e.OvertimeMinutes
    };
}
