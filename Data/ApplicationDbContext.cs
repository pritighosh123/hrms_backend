using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<LeaveRequest> LeaveRequests { get; set; } = null!;
    public DbSet<LeaveBalance> LeaveBalances { get; set; } = null!;

    public DbSet<Test>Tests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.DepartmentId);
            entity.Property(d => d.DepartmentName).IsRequired().HasMaxLength(100);
            entity.HasIndex(d => d.DepartmentName).IsUnique();
        });

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.LeaveBalance)
                  .WithOne(b => b.Employee)
                  .HasForeignKey<LeaveBalance>(b => b.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Employee)
                  .HasForeignKey<User>(u => u.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ReportsTo)
                  .WithMany(m => m.Subordinates)
                  .HasForeignKey(e => e.ReportsToEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
        });

        // Attendance configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.AttendanceId);
            entity.Property(a => a.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.Attendances)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            // One attendance record per employee per day
            entity.HasIndex(a => new { a.EmployeeId, a.Date });
        });

        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(l => l.LeaveId);
            entity.Property(l => l.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(l => l.Employee)
                  .WithMany(e => e.LeaveRequests)
                  .HasForeignKey(l => l.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // LeaveBalance configuration
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(b => b.BalanceId);
            entity.HasIndex(b => b.EmployeeId).IsUnique();
        });

        // Seed default departments
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, DepartmentName = "Human Resources" },
            new Department { DepartmentId = 2, DepartmentName = "Information Technology" },
            new Department { DepartmentId = 3, DepartmentName = "Finance" },
            new Department { DepartmentId = 4, DepartmentName = "Operations" }
        );
    }
}
