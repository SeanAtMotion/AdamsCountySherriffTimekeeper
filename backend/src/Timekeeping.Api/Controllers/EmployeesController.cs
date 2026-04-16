using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.Auth;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class EmployeesController(IProfileService profile, IEmployeeManagementService admin) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<EmployeeProfileDto>> GetMe(CancellationToken ct)
    {
        var id = User.GetEmployeeId();
        var dto = await profile.GetMyProfileAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMyProfileRequest request, CancellationToken ct)
    {
        var id = User.GetEmployeeId();
        var (ok, err) = await profile.UpdateMyProfileAsync(id, request, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<AdminEmployeeListItemDto>>> List([FromQuery] string? search, CancellationToken ct)
    {
        return Ok(await admin.ListAsync(search, ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var (ok, err, empId) = await admin.CreateAsync(request, ct);
        if (!ok) return BadRequest(new { message = err });
        return CreatedAtAction(nameof(GetById), new { id = empId }, new { employeeId = empId });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeProfileDto>> GetById(int id, CancellationToken ct)
    {
        var dto = await admin.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var (ok, err) = await admin.UpdateAsync(id, request, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] PatchEmployeeStatusRequest request, CancellationToken ct)
    {
        var (ok, err) = await admin.SetActiveAsync(id, request.IsActive, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }

    [HttpGet("{id:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> GetRole(int id, CancellationToken ct)
    {
        var dto = await admin.GetByIdAsync(id, ct);
        return dto is null ? NotFound() : Ok(new { role = dto.Role });
    }

    [HttpPut("{id:int}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] AssignRoleRequest request, CancellationToken ct)
    {
        var (ok, err) = await admin.AssignRoleAsync(id, request.Role, ct);
        if (!ok) return BadRequest(new { message = err });
        return NoContent();
    }
}
