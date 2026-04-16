using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Timekeeping.Api.Models;

namespace Timekeeping.Api.Auth;

public sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim(ClaimsPrincipalExtensions.EmployeeIdClaim, user.EmployeeId.ToString()));
        return principal;
    }
}
