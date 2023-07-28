﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimeAttendanceSystemAPI.Models;

[Table("Menu")]
public partial class Menu
{
    [Key]
    public int MenuID { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    [Required]
    [StringLength(100)]
    public string Tooltip { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string Url { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdatedAt { get; set; }

    [StringLength(100)]
    public string LastUpdatedBy { get; set; }
}