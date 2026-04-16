using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Timekeeping.Api.Auth;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Services;

namespace Timekeeping.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        if (result is null)
            return Unauthorized(new { message = "Invalid username or password, or account is inactive." });
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(ct);
        return Ok(new { message = "Signed out." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var id = User.GetEmployeeId();
        var me = await auth.GetMeAsync(id, ct);
        if (me is null) return Unauthorized();
        return Ok(me);
    }
}
