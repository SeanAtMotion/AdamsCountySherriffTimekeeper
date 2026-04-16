using Microsoft.AspNetCore.Identity;

namespace Timekeeping.Api.Models;

public sealed class ApplicationUser : IdentityUser
{
    public int EmployeeId { get; set; }
}
