namespace Timekeeping.Api.DTOs;

public sealed class EmployeeProfileDto
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleInitial { get; set; }
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Department { get; set; } = "";
    public string? Division { get; set; }
    public string JobTitle { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public string? SupervisorName { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = "";
}

public sealed class UpdateMyProfileRequest
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public sealed class AdminEmployeeListItemDto
{
    public int EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public string? Division { get; set; }
    public string? BadgeNumber { get; set; }
    public string? SupervisorName { get; set; }
    public bool IsActive { get; set; }
    public string Role { get; set; } = "";
}

public sealed class CreateEmployeeRequest
{
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleInitial { get; set; }
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string Department { get; set; } = "";
    public string? Division { get; set; }
    public string JobTitle { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public string? SupervisorName { get; set; }
    public DateOnly HireDate { get; set; }
    public string Role { get; set; } = "Employee";
}

public sealed class UpdateEmployeeRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? MiddleInitial { get; set; }
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Department { get; set; } = "";
    public string? Division { get; set; }
    public string JobTitle { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public string? SupervisorName { get; set; }
    public DateOnly HireDate { get; set; }
}

public sealed class PatchEmployeeStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class AssignRoleRequest
{
    public string Role { get; set; } = "Employee";
}
