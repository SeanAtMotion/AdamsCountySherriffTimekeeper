using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.Auth;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Employee,Admin")]
[Route("api/[controller]")]
public sealed class CorrectionsController(ICorrectionService corrections) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCorrectionRequestDto dto, CancellationToken ct)
    {
        var id = User.GetEmployeeId();
        var (ok, err) = await corrections.CreateAsync(id, dto, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<CorrectionRequestDto>>> My(CancellationToken ct)
    {
        var id = User.GetEmployeeId();
        return Ok(await corrections.MyRequestsAsync(id, ct));
    }
}
