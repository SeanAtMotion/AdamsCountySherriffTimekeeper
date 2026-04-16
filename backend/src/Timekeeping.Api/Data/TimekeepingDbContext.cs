using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Timekeeping.Api.Models;
using Timekeeping.Api.Models.Entities;

namespace Timekeeping.Api.Data;

public sealed class TimekeepingDbContext : IdentityDbContext<ApplicationUser>
{
    public TimekeepingDbContext(DbContextOptions<TimekeepingDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<CorrectionRequest> CorrectionRequests => Set<CorrectionRequest>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.EmployeeNumber).IsUnique();
            e.HasIndex(x => x.Email);
            e.HasIndex(x => x.Department);
            e.Property(x => x.EmployeeNumber).HasMaxLength(32);
            e.Property(x => x.FirstName).HasMaxLength(100);
            e.Property(x => x.LastName).HasMaxLength(100);
            e.Property(x => x.MiddleInitial).HasMaxLength(1);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.Department).HasMaxLength(128);
            e.Property(x => x.Division).HasMaxLength(128);
            e.Property(x => x.JobTitle).HasMaxLength(128);
            e.Property(x => x.BadgeNumber).HasMaxLength(32);
            e.Property(x => x.SupervisorName).HasMaxLength(150);
        });

        modelBuilder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(x => x.EmployeeId).IsUnique();
        });

        modelBuilder.Entity<TimeEntry>(e =>
        {
            e.HasOne(x => x.Employee).WithMany(x => x.TimeEntries).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EmployeeId, x.WorkDate });
            e.HasIndex(x => x.EntryStatus);
            e.HasIndex(x => x.ClockInUtc);
            e.HasIndex(x => x.ClockOutUtc);
        });

        modelBuilder.Entity<CorrectionRequest>(e =>
        {
            e.HasOne(x => x.Employee).WithMany(x => x.CorrectionRequests).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TimeEntry).WithMany(x => x.CorrectionRequests).HasForeignKey(x => x.TimeEntryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ReviewedByEmployee).WithMany().HasForeignKey(x => x.ReviewedByEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasOne(x => x.ActorEmployee).WithMany().HasForeignKey(x => x.ActorEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.TimestampUtc);
            e.HasIndex(x => new { x.EntityType, x.EntityId });
        });
    }
}
