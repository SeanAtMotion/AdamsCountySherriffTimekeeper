using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;

namespace Timekeeping.Api.Services;

public interface IProfileService
{
    Task<EmployeeProfileDto?> GetMyProfileAsync(int employeeId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateMyProfileAsync(int employeeId, UpdateMyProfileRequest req, CancellationToken ct = default);
}

public sealed class ProfileService(TimekeepingDbContext db, UserManager<ApplicationUser> userManager) : IProfileService
{
    public async Task<EmployeeProfileDto?> GetMyProfileAsync(int employeeId, CancellationToken ct = default)
    {
        var e = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == employeeId, ct);
        return e is null ? null : EmployeeMapper.ToProfileDto(e);
    }

    public async Task<(bool Success, string? Error)> UpdateMyProfileAsync(int employeeId, UpdateMyProfileRequest req, CancellationToken ct = default)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);
        if (employee is null) return (false, "Employee not found.");

        if (!string.IsNullOrWhiteSpace(req.Phone))
            employee.Phone = req.Phone.Trim();

        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            if (await db.Employees.AnyAsync(e => e.Email == req.Email && e.EmployeeId != employeeId, ct))
                return (false, "Email already in use.");
            employee.Email = req.Email.Trim();
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId, ct);
            if (user is not null)
            {
                user.Email = employee.Email;
                await userManager.UpdateAsync(user);
            }
        }

        employee.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }
}
