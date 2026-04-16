using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<MeResponse?> GetMeAsync(int employeeId, CancellationToken ct = default);
}

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    TimekeepingDbContext db) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByNameAsync(request.Username);
        if (user is null) return null;

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId, ct);
        if (employee is null || !employee.IsActive) return null;

        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (result != SignInResult.Success) return null;

        return new LoginResponse
        {
            User = new UserSummaryDto
            {
                EmployeeId = employee.EmployeeId,
                UserName = user.UserName ?? "",
                FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
                Role = employee.Role == AppRole.Admin ? "Admin" : "Employee"
            }
        };
    }

    public Task LogoutAsync(CancellationToken ct = default) => signInManager.SignOutAsync();

    public async Task<MeResponse?> GetMeAsync(int employeeId, CancellationToken ct = default)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);
        if (employee is null) return null;
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId, ct);
        if (user is null) return null;

        return new MeResponse
        {
            EmployeeId = employee.EmployeeId,
            UserName = user.UserName ?? "",
            FullName = $"{employee.FirstName} {employee.LastName}".Trim(),
            Role = employee.Role == AppRole.Admin ? "Admin" : "Employee",
            Email = employee.Email
        };
    }
}
