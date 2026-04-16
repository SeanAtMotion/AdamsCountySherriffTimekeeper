using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reports")]
public sealed class AdminReportsController(IReportService reports) : ControllerBase
{
    [HttpGet("hours-summary")]
    public async Task<ActionResult<IReadOnlyList<HoursSummaryRowDto>>> HoursSummary(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        return Ok(await reports.HoursSummaryAsync(from, to, ct));
    }

    [HttpGet("overtime")]
    public async Task<ActionResult<IReadOnlyList<OvertimeRowDto>>> Overtime(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        return Ok(await reports.OvertimeAsync(from, to, ct));
    }

    [HttpGet("missing-punches")]
    public async Task<ActionResult<IReadOnlyList<MissingPunchRowDto>>> MissingPunches(CancellationToken ct)
    {
        return Ok(await reports.MissingPunchesAsync(ct));
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<IReadOnlyList<AttendanceSummaryRowDto>>> Attendance(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        return Ok(await reports.AttendanceAsync(from, to, ct));
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
    {
        var bytes = await reports.ExportCsvAsync(from, to, ct);
        return File(bytes, "text/csv", $"hours-summary-{from:yyyyMMdd}-{to:yyyyMMdd}.csv");
    }
}
