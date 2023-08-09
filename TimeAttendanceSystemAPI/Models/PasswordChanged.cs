﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimeAttendanceSystemAPI.Models;

[Table("PasswordChanged")]
public partial class PasswordChanged
{
    [Key]
    public int PasswordChangedID { get; set; }

    public Guid UserID { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string OldPassword { get; set; }

    [ForeignKey("UserID")]
    [InverseProperty("PasswordChangeds")]
    public virtual TbUser User { get; set; }
}