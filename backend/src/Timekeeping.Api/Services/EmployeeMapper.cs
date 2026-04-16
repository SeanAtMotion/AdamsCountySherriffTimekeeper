using Timekeeping.Api.DTOs;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Services;

public static class EmployeeMapper
{
    public static EmployeeProfileDto ToProfileDto(Employee e) => new()
    {
        EmployeeId = e.EmployeeId,
        EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName,
        LastName = e.LastName,
        MiddleInitial = e.MiddleInitial,
        Email = e.Email,
        Phone = e.Phone,
        Department = e.Department,
        Division = e.Division,
        JobTitle = e.JobTitle,
        BadgeNumber = e.BadgeNumber,
        SupervisorName = e.SupervisorName,
        HireDate = e.HireDate,
        IsActive = e.IsActive,
        Role = e.Role == AppRole.Admin ? "Admin" : "Employee"
    };

    public static AdminEmployeeListItemDto ToAdminListItem(Employee e) => new()
    {
        EmployeeId = e.EmployeeId,
        EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Department = e.Department,
        Division = e.Division,
        BadgeNumber = e.BadgeNumber,
        SupervisorName = e.SupervisorName,
        IsActive = e.IsActive,
        Role = e.Role == AppRole.Admin ? "Admin" : "Employee"
    };
}
