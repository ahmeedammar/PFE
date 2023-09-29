//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using System.IO;
//using System.Data;
//using ExcelDataReader;
//using Expense_Tracker.Filters;

//namespace Expense_Tracker.Controllers
//{
//    [AuthorizeAdmin]
//    public class ImportController : Controller
//    {
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public ImportController(IWebHostEnvironment webHostEnvironment)
//        {
//            _webHostEnvironment = webHostEnvironment;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult ProcessFile(IFormFile file)
//        {
//            // Check if a file is selected
//            if (file == null || file.Length == 0)
//            {
//                ModelState.AddModelError("File", "Please select a file.");
//                return View("Index");
//            }

//            // Verify file extension
//            var fileExtension = Path.GetExtension(file.FileName);
//            if (fileExtension.ToLower() != ".xls")
//            {
//                ModelState.AddModelError("File", "Invalid file format. Only .xls files are supported.");
//                return View("Index");
//            }

//            // Save the uploaded file
//            var uploadsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
//            var filePath = Path.Combine(uploadsFolder, file.FileName);
//            using (var fileStream = new FileStream(filePath, FileMode.Create))
//            {
//                file.CopyTo(fileStream);
//            }

//            // Read the Excel file
//            var projects = new DataTable();
//            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
//            {
//                using (var reader = ExcelReaderFactory.CreateBinaryReader(stream))
//                {
//                    projects.Load(reader, LoadOption.OverwriteChanges);
//                }
//            }

//            return View("Result", projects);
//        }
//    }
//}
