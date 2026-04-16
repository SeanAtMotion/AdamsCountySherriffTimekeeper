namespace Timekeeping.Api.DTOs;

public sealed class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

/// <summary>Returned after cookie sign-in; session is the auth cookie (HttpOnly).</summary>
public sealed class LoginResponse
{
    public UserSummaryDto User { get; set; } = null!;
}

public sealed class UserSummaryDto
{
    public int EmployeeId { get; set; }
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}

public sealed class MeResponse
{
    public int EmployeeId { get; set; }
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public string Email { get; set; } = "";
}
