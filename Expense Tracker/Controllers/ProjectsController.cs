using System.IO;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NReco.PdfGenerator;

namespace Expense_Tracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;

        public ProjectsController(ApplicationDbContext context, ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _context = context;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
        }
        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Employee)
                .ToListAsync();
            return View(projects);
        }
        private string GenerateInvoiceHtml(InvoiceViewModel invoice)
        {
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = invoice
            };

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, "GenerateInvoice", false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException("GenerateInvoice view cannot be found");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    viewData,
                    new TempDataDictionary(ControllerContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                return sw.ToString();
            }
        }

        private byte[] GeneratePdf(string htmlContent)
        {
            var converter = new HtmlToPdfConverter();

            var pdfBytes = converter.GeneratePdf(htmlContent);

            return pdfBytes;
        }

        // GET: Projects/AddOrEdit/5
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                ViewBag.Clients = new SelectList(_context.Clients, "ClientId", "Name");
                ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name");
                return View(new Project());
            }
            else
            {
                var project = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(p => p.ProjectId == id);

                if (project == null)
                {
                    return NotFound();
                }

                ViewBag.Clients = new SelectList(_context.Clients, "ClientId", "Name", project.ClientId);
                ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name", project.EmployeeId);
                return View(project);
            }
        }

        // GET: Projects/GetEmployeeHourlyRate
        [HttpGet]
        public async Task<IActionResult> GetEmployeeHourlyRate(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee != null)
            {
                return Ok(employee.HourlyRate);
            }
            return BadRequest("Employee not found.");
        }

        //POST AddOrEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(Project project)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var employee = await _context.Employees.FindAsync(project.EmployeeId);
                    if (employee == null)
                    {
                        return BadRequest("Invalid employee selection.");
                    }

                    if (project.ProjectId == 0)
                    {
                        project.Total = employee.HourlyRate * project.HoursOfWork;
                        _context.Projects.Add(project);
                    }
                    else
                    {
                        var existingProject = await _context.Projects.FindAsync(project.ProjectId);
                        if (existingProject == null)
                        {
                            return NotFound();
                        }

                        existingProject.Name = project.Name;
                        existingProject.ClientId = project.ClientId;
                        existingProject.EmployeeId = project.EmployeeId;
                        existingProject.HoursOfWork = project.HoursOfWork;
                        existingProject.Total = employee.HourlyRate * project.HoursOfWork;
                    }

                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId))
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
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the project.");

                    // Log the exception
                    Console.WriteLine(ex.ToString());

                    // Rethrow the exception
                    throw;
                }
            }

            ViewBag.Clients = new SelectList(_context.Clients, "ClientId", "Name", project.ClientId);
            ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name", project.EmployeeId);

            return View(project);
        }

        public IActionResult GenerateInvoice(int id)
        {
            var project = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Employee)
                .FirstOrDefault(p => p.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            var invoiceViewModel = new InvoiceViewModel
            {
                ProjectName = project.Name,
                ClientName = project.Client.Name,
                EmployeeName = project.Employee.Name,
                HoursOfWork = project.HoursOfWork,
                HourlyRate = project.Employee.HourlyRate,
                Total = project.Total ?? 0
            };

            var htmlContent = GenerateInvoiceHtml(invoiceViewModel);
            var pdfBytes = GeneratePdf(htmlContent);

            return File(pdfBytes, "application/pdf", "Invoice.pdf");
        }

        

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
