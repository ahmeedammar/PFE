using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_Tracker.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }



        // GET: Employees/AddOrEdit/5
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Employee());
            }
            else
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                {
                    return NotFound();
                }

                return View(employee);
            }
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ImportHourlyRates(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                // Invalid or empty file
                TempData["ImportResultMessage"] = "Please choose a valid file.";
                return RedirectToAction("Index");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            var rowCount = worksheet.Dimension.Rows;
                            var hourlyRates = new List<(string employeeName, decimal hourlyRate)>();

                            // Assuming that the header is in cells (1,1) and (1,2)
                            var header1 = worksheet.Cells[1, 1]?.Value?.ToString()?.Trim();
                            var header2 = worksheet.Cells[2, 1]?.Value?.ToString()?.Trim();

                            for (int row = 2; row <= rowCount; row++) // Start from row 2
                            {
                                var employeeName = worksheet.Cells[row, 1]?.Value?.ToString()?.Trim();
                                var hourlyRateString = worksheet.Cells[row, 2]?.Value?.ToString()?.Trim();

                                if (!string.IsNullOrEmpty(employeeName) && decimal.TryParse(hourlyRateString, out var hourlyRate))
                                {
                                    hourlyRates.Add((employeeName, hourlyRate));
                                }
                            }

                            // Use hourlyRates list to update the hourly rates for employees in the database
                            foreach (var (employeeName, hourlyRate) in hourlyRates)
                            {
                                var existingEmployee = _context.Employees.FirstOrDefault(e => e.Name == employeeName);
                                if (existingEmployee != null)
                                {
                                    // Employee exists, update hourly rate
                                    existingEmployee.HourlyRate = hourlyRate;
                                    _context.Employees.Update(existingEmployee);
                                }
                                else
                                {
                                    // Employee doesn't exist, create a new record
                                    var newEmployee = new Employee
                                    {
                                        Name = employeeName,
                                        HourlyRate = hourlyRate
                                    };
                                    _context.Employees.Add(newEmployee);
                                }
                            }

                            // Save changes to the database
                            await _context.SaveChangesAsync();

                            TempData["ImportResultMessage"] = "Hourly rates imported successfully.";
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during file import
                TempData["ImportResultMessage"] = $"An error occurred during the import process: {ex.Message}";
                return RedirectToAction("Index");
            }

            TempData["ImportResultMessage"] = "Invalid file format or an error occurred during the import process.";
            return RedirectToAction("Index");
        }

        // POST: Employees/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (employee.EmployeeId == 0)
                    {
                        _context.Employees.Add(employee);
                    }
                    else
                    {
                        _context.Employees.Update(employee);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the employee.");

                    // Log the exception
                    Console.WriteLine(ex.ToString());

                    // Rethrow the exception
                    throw;
                }
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
