using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Data;
using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public interface IEmployeeManagementService
{
    Task<IReadOnlyList<AdminEmployeeListItemDto>> ListAsync(string? search, CancellationToken ct = default);
    Task<EmployeeProfileDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(bool Success, string? Error, int? EmployeeId)> CreateAsync(CreateEmployeeRequest req, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateEmployeeRequest req, CancellationToken ct = default);
    Task<(bool Success, string? Error)> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
    Task<(bool Success, string? Error)> AssignRoleAsync(int id, string role, CancellationToken ct = default);
}

public sealed class EmployeeManagementService(
    TimekeepingDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IEmployeeManagementService
{
    public async Task<IReadOnlyList<AdminEmployeeListItemDto>> ListAsync(string? search, CancellationToken ct = default)
    {
        var q = db.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(e =>
                e.FirstName.Contains(s) || e.LastName.Contains(s) || e.EmployeeNumber.Contains(s) ||
                e.Email.Contains(s) || e.Department.Contains(s) ||
                (e.Division != null && e.Division.Contains(s)) ||
                (e.BadgeNumber != null && e.BadgeNumber.Contains(s)) ||
                (e.SupervisorName != null && e.SupervisorName.Contains(s)));
        }

        var list = await q.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync(ct);
        return list.Select(EmployeeMapper.ToAdminListItem).ToList();
    }

    public async Task<EmployeeProfileDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var e = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == id, ct);
        return e is null ? null : EmployeeMapper.ToProfileDto(e);
    }

    public async Task<(bool Success, string? Error, int? EmployeeId)> CreateAsync(CreateEmployeeRequest req, CancellationToken ct = default)
    {
        if (await db.Employees.AnyAsync(e => e.EmployeeNumber == req.EmployeeNumber, ct))
            return (false, "Employee number already exists.", null);
        if (await db.Employees.AnyAsync(e => e.Email == req.Email, ct))
            return (false, "Email already exists.", null);
        if (await userManager.FindByNameAsync(req.UserName) is not null)
            return (false, "Username already exists.", null);

        var role = req.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? AppRole.Admin : AppRole.Employee;

        var employee = new Employee
        {
            EmployeeNumber = req.EmployeeNumber.Trim(),
            FirstName = req.FirstName.Trim(),
            LastName = req.LastName.Trim(),
            MiddleInitial = string.IsNullOrWhiteSpace(req.MiddleInitial) ? null : req.MiddleInitial.Trim()[..1],
            Email = req.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim(),
            Department = req.Department.Trim(),
            Division = string.IsNullOrWhiteSpace(req.Division) ? null : req.Division.Trim(),
            JobTitle = req.JobTitle.Trim(),
            BadgeNumber = string.IsNullOrWhiteSpace(req.BadgeNumber) ? null : req.BadgeNumber.Trim(),
            SupervisorName = string.IsNullOrWhiteSpace(req.SupervisorName) ? null : req.SupervisorName.Trim(),
            HireDate = req.HireDate,
            IsActive = true,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        var user = new ApplicationUser
        {
            UserName = req.UserName.Trim(),
            Email = req.Email.Trim(),
            EmployeeId = employee.EmployeeId,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)), null);

        var identityRole = role == AppRole.Admin ? "Admin" : "Employee";
        await EnsureRoleExistsAsync(identityRole);
        await userManager.AddToRoleAsync(user, identityRole);

        return (true, null, employee.EmployeeId);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateEmployeeRequest req, CancellationToken ct = default)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id, ct);
        if (employee is null) return (false, "Employee not found.");

        if (!string.Equals(employee.Email, req.Email, StringComparison.OrdinalIgnoreCase)
            && await db.Employees.AnyAsync(e => e.Email == req.Email && e.EmployeeId != id, ct))
            return (false, "Email already exists.");

        employee.FirstName = req.FirstName.Trim();
        employee.LastName = req.LastName.Trim();
        employee.MiddleInitial = string.IsNullOrWhiteSpace(req.MiddleInitial) ? null : req.MiddleInitial.Trim()[..1];
        employee.Email = req.Email.Trim();
        employee.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        employee.Department = req.Department.Trim();
        employee.Division = string.IsNullOrWhiteSpace(req.Division) ? null : req.Division.Trim();
        employee.JobTitle = req.JobTitle.Trim();
        employee.BadgeNumber = string.IsNullOrWhiteSpace(req.BadgeNumber) ? null : req.BadgeNumber.Trim();
        employee.SupervisorName = string.IsNullOrWhiteSpace(req.SupervisorName) ? null : req.SupervisorName.Trim();
        employee.HireDate = req.HireDate;
        employee.UpdatedAtUtc = DateTime.UtcNow;

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id, ct);
        if (user is not null && user.Email != employee.Email)
        {
            user.Email = employee.Email;
            await userManager.UpdateAsync(user);
        }

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id, ct);
        if (employee is null) return (false, "Employee not found.");
        employee.IsActive = isActive;
        employee.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> AssignRoleAsync(int id, string role, CancellationToken ct = default)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id, ct);
        if (employee is null) return (false, "Employee not found.");

        var appRole = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? AppRole.Admin : AppRole.Employee;
        employee.Role = appRole;
        employee.UpdatedAtUtc = DateTime.UtcNow;

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id, ct);
        if (user is null) return (false, "User account missing.");

        var current = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, current);

        var identityRole = appRole == AppRole.Admin ? "Admin" : "Employee";
        await EnsureRoleExistsAsync(identityRole);
        await userManager.AddToRoleAsync(user, identityRole);

        await db.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
