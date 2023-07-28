﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimeAttendanceSystemAPI.Models;

[Table("Violation")]
public partial class Violation
{
    [Key]
    public int ViolationID { get; set; }

    [Required]
    [StringLength(50)]
    public string TypeOfViolation { get; set; }

    [Column(TypeName = "money")]
    public decimal AmountDeducted { get; set; }

    [InverseProperty("Violation")]
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}