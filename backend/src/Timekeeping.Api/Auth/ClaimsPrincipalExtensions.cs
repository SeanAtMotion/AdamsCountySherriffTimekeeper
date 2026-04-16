using System.Security.Claims;

namespace Timekeeping.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public const string EmployeeIdClaim = "employee_id";

    public static int GetEmployeeId(this ClaimsPrincipal user)
    {
        var v = user.FindFirstValue(EmployeeIdClaim);
        if (string.IsNullOrEmpty(v) || !int.TryParse(v, out var id))
            throw new UnauthorizedAccessException("Invalid employee context.");
        return id;
    }

    public static bool TryGetEmployeeId(this ClaimsPrincipal user, out int employeeId)
    {
        employeeId = 0;
        var v = user.FindFirstValue(EmployeeIdClaim);
        return !string.IsNullOrEmpty(v) && int.TryParse(v, out employeeId);
    }
}
