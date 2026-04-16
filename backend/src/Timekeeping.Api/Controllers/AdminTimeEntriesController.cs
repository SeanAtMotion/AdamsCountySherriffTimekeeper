using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.Auth;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/timeentries")]
public sealed class AdminTimeEntriesController(IAdminTimeEntryService admin) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TimeEntryDto>>> List([FromQuery] AdminTimeEntryQuery query, CancellationToken ct)
    {
        return Ok(await admin.QueryAsync(query, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TimeEntryDto>> Get(int id, CancellationToken ct)
    {
        var dto = await admin.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateTimeEntryRequest request, CancellationToken ct)
    {
        var actor = User.GetEmployeeId();
        var (ok, err) = await admin.UpdateAsync(actor, id, request, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }
}
