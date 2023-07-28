﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TimeAttendanceSystemAPI.Models;

public partial class TimeAttendanceSystemContext : DbContext
{
    public TimeAttendanceSystemContext(DbContextOptions<TimeAttendanceSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleMenu> RoleMenus { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<TbUser> TbUsers { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeID).HasName("employee_employeeid_primary");

            entity.Property(e => e.EmployeeID).ValueGeneratedNever();
            entity.Property(e => e.Gender).IsFixedLength();
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuID).HasName("menu_menuid_primary");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.PayRollID).HasName("payroll_payrollid_primary");

            entity.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payroll_Employee");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportID).HasName("report_reportid_primary");

            entity.HasOne(d => d.Employee).WithMany(p => p.Reports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Report_Employee");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleID).HasName("role_roleid_primary");
        });

        modelBuilder.Entity<RoleMenu>(entity =>
        {
            entity.HasKey(e => new { e.MenuID, e.RoleID }).HasName("rolemenu_roleid_menuid_primary");
            entity.HasOne(d => d.Menu).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleMenu_Menu");

            entity.HasOne(d => d.Role).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoleMenu_Role");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleID).HasName("schedule_scheduleid_primary");

            entity.Property(e => e.ScheduleID).ValueGeneratedNever();

            entity.HasOne(d => d.Employee).WithMany(p => p.Schedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Employee");

            entity.HasOne(d => d.Violation).WithMany(p => p.Schedules).HasConstraintName("FK_Schedule_Violation");
        });

        modelBuilder.Entity<TbUser>(entity =>
        {
            entity.HasKey(e => e.UserID).HasName("user_userid_primary");

            entity.Property(e => e.UserID).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserID, e.RoleID }).HasName("userrole_userid_roleid_primary");
            entity.HasOne(d => d.Role).WithMany()
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRole_Role");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationID).HasName("violation_violationid_primary");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}