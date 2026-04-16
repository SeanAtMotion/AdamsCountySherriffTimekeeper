using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.Auth;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/corrections")]
public sealed class AdminCorrectionsController(ICorrectionService corrections) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CorrectionRequestDto>>> List([FromQuery] CorrectionRequestStatus? status, CancellationToken ct)
    {
        return Ok(await corrections.AdminListAsync(status, ct));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewCorrectionRequest review, CancellationToken ct)
    {
        var adminId = User.GetEmployeeId();
        var (ok, err) = await corrections.ApproveAsync(adminId, id, review, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }

    [HttpPost("{id:int}/deny")]
    public async Task<IActionResult> Deny(int id, [FromBody] ReviewCorrectionRequest review, CancellationToken ct)
    {
        var adminId = User.GetEmployeeId();
        var (ok, err) = await corrections.DenyAsync(adminId, id, review, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }
}
