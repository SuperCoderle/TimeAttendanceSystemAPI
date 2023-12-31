﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TimeAttendanceSystemAPI.Models;

[Keyless]
[Table("RoleMenu")]
public partial class RoleMenu
{
    public int MenuID { get; set; }

    public int RoleID { get; set; }

    [ForeignKey("MenuID")]
    public virtual Menu Menu { get; set; }

    [ForeignKey("RoleID")]
    public virtual Role Role { get; set; }
}