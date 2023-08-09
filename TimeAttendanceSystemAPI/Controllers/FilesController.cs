using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using TimeAttendanceSystemAPI.Models;

namespace TimeAttendanceSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IWebHostEnvironment _web;
        private readonly TimeAttendanceSystemContext _context;
        private IExcelDataReader _excelDataReader;

        public FilesController(IWebHostEnvironment web, TimeAttendanceSystemContext context)
        {
            _web = web;
            _context = context;
        }

        [HttpPost("{type}")]
        public async Task<IActionResult> PostFile(string type)
        {
            try
            {
                var file = Request.Form.Files[0];
                if (file == null)
                {
                    return NotFound("Không tìm thấy file");
                }

                string path = Path.Combine(_web.WebRootPath, "ReceivedFiles");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Path.GetFileName(file.FileName);

                string extension = Path.GetExtension(fileName);

                string[] allowedExtension = new string[] { ".xls", ".xlsx" };
                if (!allowedExtension.Contains(extension))
                {
                    return BadRequest("This file's not allowed.");
                }

                string savePath = Path.Combine(path, fileName);

                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = new FileStream(savePath, FileMode.Open))
                {
                    if (extension == ".xls")
                        _excelDataReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else
                        _excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                    DataSet ds = new DataSet();
                    ds = _excelDataReader.AsDataSet();
                    _excelDataReader.Close();

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        switch (type)
                        {
                            case "Employee":
                                DataTable serviceEmployee = ds.Tables[0];
                                for (int i = 1; i < serviceEmployee.Rows.Count; i++)
                                {
                                    Employee newEmp = new Employee()
                                    {
                                        EmployeeID = Guid.NewGuid(),
                                        Fullname = serviceEmployee.Rows[i][0].ToString(),
                                        Birthday = Convert.ToDateTime(serviceEmployee.Rows[i][1].ToString()),
                                        Gender = serviceEmployee.Rows[i][2].ToString(),
                                        PhoneNumber = serviceEmployee.Rows[i][3].ToString(),
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = User.FindFirstValue("fullname"),
                                        LastUpdatedAt = null,
                                        LastUpdatedBy = null
                                    };

                                    await _context.Employees.AddAsync(newEmp);
                                }
                                break;
                            case "Menu":
                                DataTable serviceMenu = ds.Tables[0];
                                for (int i = 1; i < serviceMenu.Rows.Count; i++)
                                {
                                    Menu newMenu = new Menu()
                                    {
                                        MenuID = 0,
                                        Title = serviceMenu.Rows[i][0].ToString(),
                                        Icon = serviceMenu.Rows[i][1].ToString(),
                                        Url = serviceMenu.Rows[i][2].ToString(),
                                        ParentID = int.Parse(serviceMenu.Rows[i][3].ToString()!),
                                        IsActive = Convert.ToBoolean(serviceMenu.Rows[i][4].ToString()),
                                        IsSubmenu = Convert.ToBoolean(serviceMenu.Rows[i][5].ToString()),
                                        CreatedAt = DateTime.Now,
                                        LastUpdatedAt = null,
                                        LastUpdatedBy = null
                                    };

                                    await _context.Menus.AddAsync(newMenu);
                                }
                                break;
                            case "Shift":
                                DataTable serviceShift = ds.Tables[0];
                                for (int i = 1; i < serviceShift.Rows.Count; i++)
                                {
                                    Shift newShift = new Shift()
                                    {
                                        ShiftID = 0,
                                        ShiftName = serviceShift.Rows[i][0].ToString(),
                                        StartTime = TimeSpan.Parse(serviceShift.Rows[i][1].ToString() == null ? "00:00:00" : serviceShift.Rows[i][1].ToString()!),
                                        EndTime = TimeSpan.Parse(serviceShift.Rows[i][2].ToString() == null ? "00:00:00" : serviceShift.Rows[i][2].ToString()!)
                                    };

                                    await _context.Shifts.AddAsync(newShift);
                                }
                                break;
                            case "Violation":
                                DataTable serviceViolation = ds.Tables[0];
                                for (int i = 1; i < serviceViolation.Rows.Count; i++)
                                {
                                    Violation newViolation = new Violation()
                                    {
                                        ViolationID = 0,
                                        ViolationError = serviceViolation.Rows[i][0].ToString(),
                                    };

                                    await _context.Violations.AddAsync(newViolation);
                                }
                                break;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
