using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Timekeeping.Api.Auth;
using Timekeeping.Api.Data;
using Timekeeping.Api.Models;
using Timekeeping.Api.Options;
using Timekeeping.Api.Services;
using Timekeeping.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust proxy headers from Azure App Service, IIS Application Request Routing, etc.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
    cfg.Enrich.FromLogContext();
    if (!ctx.HostingEnvironment.IsDevelopment())
        return;
    cfg.MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning);
});

builder.Services.Configure<TimekeepingOptions>(builder.Configuration.GetSection(TimekeepingOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

if (builder.Environment.IsProduction()
    && connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Production is configured with a LocalDB connection string. Set ConnectionStrings__DefaultConnection to your SQL Server (environment variable or Azure connection string).");
}

builder.Services.AddDbContext<TimekeepingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<TimekeepingDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Timekeeping.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction()
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = PathString.Empty;
    options.AccessDeniedPath = PathString.Empty;
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(LoginRequestValidator).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Adams County SO Timekeeping API",
        Version = "v1",
        Description = "Uses ASP.NET Core Identity cookie authentication. Sign in via the web app; Swagger does not attach cookies automatically.",
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IAdminTimeEntryService, AdminTimeEntryService>();
builder.Services.AddScoped<ICorrectionService, CorrectionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddSingleton<IOfficeTimeProvider, OfficeTimeProvider>();

builder.Services.AddCors(o =>
{
    o.AddPolicy("Frontend", p =>
    {
        p.WithOrigins(
                builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

await DataSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
// Skip in Development: Vite proxies HTTP-only; HTTPS redirect would move the browser to another
// origin (7222) and the session cookie for :5173 would not be sent.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
