using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Timekeeping.Api.Auth;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Options;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Employee,Admin")]
[Route("api/[controller]")]
public sealed class TimeEntriesController(
    TimekeepingDbContext db,
    ITimeEntryService time,
    IOptions<TimekeepingOptions> options) : ControllerBase
{
    private readonly TimekeepingOptions _opt = options.Value;

    [HttpGet("dashboard")]
    public async Task<ActionResult<EmployeeDashboardDto>> Dashboard(CancellationToken ct)
    {
        var employeeId = User.GetEmployeeId();
        var state = await time.GetSessionStateAsync(employeeId, ct);
        var active = await time.GetActiveOpenEntryAsync(employeeId, ct);
        var recent = await db.TimeEntries.AsNoTracking()
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.ClockInUtc)
            .Take(10)
            .ToListAsync(ct);

        return Ok(new EmployeeDashboardDto
        {
            SessionState = state,
            ActiveEntry = active is null ? null : TimeEntryMapper.ToDto(active),
            BreaksEnabled = _opt.EnableMealBreaks,
            RecentEntries = recent.Select(TimeEntryMapper.ToDto).ToList()
        });
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<TimeEntryDto>> ClockIn(CancellationToken ct)
    {
        try
        {
            var entry = await time.ClockInAsync(User.GetEmployeeId(), ct);
            return Ok(TimeEntryMapper.ToDto(entry));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<TimeEntryDto>> ClockOut(CancellationToken ct)
    {
        try
        {
            var entry = await time.ClockOutAsync(User.GetEmployeeId(), ct);
            return Ok(TimeEntryMapper.ToDto(entry));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("break-start")]
    public async Task<ActionResult<TimeEntryDto>> BreakStart(CancellationToken ct)
    {
        try
        {
            var entry = await time.BreakStartAsync(User.GetEmployeeId(), ct);
            return Ok(TimeEntryMapper.ToDto(entry));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("break-end")]
    public async Task<ActionResult<TimeEntryDto>> BreakEnd(CancellationToken ct)
    {
        try
        {
            var entry = await time.BreakEndAsync(User.GetEmployeeId(), ct);
            return Ok(TimeEntryMapper.ToDto(entry));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<TimeEntryDto>>> My(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var employeeId = User.GetEmployeeId();
        var q = db.TimeEntries.AsNoTracking().Where(e => e.EmployeeId == employeeId);
        if (from is not null) q = q.Where(e => e.WorkDate >= from);
        if (to is not null) q = q.Where(e => e.WorkDate <= to);
        var list = await q.OrderByDescending(e => e.WorkDate).ThenByDescending(e => e.ClockInUtc).Take(500).ToListAsync(ct);
        return Ok(list.Select(TimeEntryMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimeEntryDto>> GetById(int id, CancellationToken ct)
    {
        var employeeId = User.GetEmployeeId();
        var e = await db.TimeEntries.AsNoTracking().FirstOrDefaultAsync(x => x.TimeEntryId == id && x.EmployeeId == employeeId, ct);
        return e is null ? NotFound() : Ok(TimeEntryMapper.ToDto(e));
    }
}
