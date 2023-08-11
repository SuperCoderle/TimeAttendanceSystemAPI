using Microsoft.EntityFrameworkCore;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Services
{
    public class CheckIdentService
    {
        private readonly TimeAttendanceSystemContext _context;

        public CheckIdentService(TimeAttendanceSystemContext context)
        {
            _context = context;
        }

        public void CheckIdentPayroll()
        {
            int maxColumn = _context.Payrolls.OrderByDescending(x => x.PayRollID).FirstOrDefault() != null ? _context.Payrolls.OrderByDescending(x => x.PayRollID).FirstOrDefault()!.PayRollID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Payroll, RESEED, {maxColumn})");
        }

        public void CheckIdentMenu()
        {
            int maxColumn = _context.Menus.OrderByDescending(x => x.MenuID).FirstOrDefault() != null ? _context.Menus.OrderByDescending(x => x.MenuID).FirstOrDefault()!.MenuID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Menu, RESEED, {maxColumn})");
        }

        public void CheckIdentShift()
        {
            int maxColumn = _context.Shifts.OrderByDescending(x => x.ShiftID).FirstOrDefault() != null ? _context.Shifts.OrderByDescending(x => x.ShiftID).FirstOrDefault()!.ShiftID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Shift, RESEED, {maxColumn})");
        }

        public void CheckIdentViolation()
        {
            int maxColumn = _context.Violations.OrderByDescending(x => x.ViolationID).FirstOrDefault() != null ? _context.Violations.OrderByDescending(x => x.ViolationID).FirstOrDefault()!.ViolationID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Violation, RESEED, {maxColumn})");
        }

        public void CheckIdentReport()
        {
            int maxColumn = _context.Reports.OrderByDescending(x => x.ReportID).FirstOrDefault() != null ? _context.Reports.OrderByDescending(x => x.ReportID).FirstOrDefault()!.ReportID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (Report, RESEED, {maxColumn})");
        }

        public void CheckIdentPassword()
        {
            int maxColumn = _context.PasswordChangeds.OrderByDescending(x => x.PasswordChangedID).FirstOrDefault() != null ? _context.PasswordChangeds.OrderByDescending(x => x.PasswordChangedID).FirstOrDefault()!.PasswordChangedID : 0;
            _context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (PasswordChanged, RESEED, {maxColumn})");
        }
    }
}
