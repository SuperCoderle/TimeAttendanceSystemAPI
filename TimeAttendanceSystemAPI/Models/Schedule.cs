﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimeAttendanceSystemAPI.Models;

[Table("Schedule")]
public partial class Schedule
{
    [Key]
    public Guid ScheduleID { get; set; }

    public int? ShiftID { get; set; }

    public TimeSpan TimeIn { get; set; }

    public TimeSpan TimeOut { get; set; }

    [Column(TypeName = "date")]
    public DateTime WorkDate { get; set; }

    [Column(TypeName = "ntext")]
    public string Description { get; set; }

    public Guid EmployeeID { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal TotalWorkHours { get; set; }

    public bool IsInProgress { get; set; }

    public bool IsSubmit { get; set; }

    public bool IsOpen { get; set; }

    public int? ViolationID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ApprovedAt { get; set; }

    [StringLength(100)]
    public string ApprovedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; }

    [ForeignKey("EmployeeID")]
    [InverseProperty("Schedules")]
    public virtual Employee Employee { get; set; }

    [ForeignKey("ShiftID")]
    [InverseProperty("Schedules")]
    public virtual Shift Shift { get; set; }

    [ForeignKey("ViolationID")]
    [InverseProperty("Schedules")]
    public virtual Violation Violation { get; set; }
}