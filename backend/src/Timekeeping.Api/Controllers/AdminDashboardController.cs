using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController(IAdminDashboardService dashboard) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> Stats(CancellationToken ct)
    {
        return Ok(await dashboard.GetStatsAsync(ct));
    }
}
