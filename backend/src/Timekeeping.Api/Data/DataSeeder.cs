using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TimekeepingDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "Employee" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (await db.Employees.AnyAsync())
            return;

        var adminEmp = new Employee
        {
            EmployeeNumber = "ADMIN001",
            FirstName = "System",
            LastName = "Administrator",
            Email = "admin@adamscounty.local",
            Phone = "717-555-0100",
            Department = "Sheriff Administration",
            Division = "IT",
            JobTitle = "System Administrator",
            BadgeNumber = null,
            SupervisorName = "Sheriff (Office of Sheriff)",
            HireDate = new DateOnly(2020, 1, 1),
            IsActive = true,
            Role = AppRole.Admin,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var emp1 = new Employee
        {
            EmployeeNumber = "S001",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@adamscounty.local",
            Department = "Patrol",
            Division = "North",
            JobTitle = "Deputy",
            BadgeNumber = "1001",
            SupervisorName = "Sgt. A. Miller",
            HireDate = new DateOnly(2021, 6, 1),
            IsActive = true,
            Role = AppRole.Employee,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var emp2 = new Employee
        {
            EmployeeNumber = "S002",
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@adamscounty.local",
            Department = "Corrections",
            Division = "Booking",
            JobTitle = "Corrections Officer",
            BadgeNumber = "2100",
            SupervisorName = "Lt. R. Carter",
            HireDate = new DateOnly(2022, 3, 15),
            IsActive = true,
            Role = AppRole.Employee,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Employees.AddRange(adminEmp, emp1, emp2);
        await db.SaveChangesAsync();

        async Task CreateUserAsync(Employee e, string userName, string password, string role)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = e.Email,
                EmployeeId = e.EmployeeId,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(er => er.Description)));
            await userManager.AddToRoleAsync(user, role);
        }

        await CreateUserAsync(adminEmp, "admin", "ChangeMe!123", "Admin");
        await CreateUserAsync(emp1, "jdoe", "ChangeMe!123", "Employee");
        await CreateUserAsync(emp2, "jsmith", "ChangeMe!123", "Employee");
    }
}
