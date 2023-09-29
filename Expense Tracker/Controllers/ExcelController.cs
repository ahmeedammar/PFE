using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Expense_Tracker.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Expense_Tracker.Controllers
{
    public class ExcelController : Controller
    {
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                ViewBag.ErrorMessage = "Please select a file to upload.";
                return View();
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    IWorkbook workbook = new XSSFWorkbook(stream);
                    ISheet sheet = workbook.GetSheetAt(0); // Assuming the data is in the first sheet

                    List<ExcelDataViewModel> excelDataList = new List<ExcelDataViewModel>();

                    for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        IRow row = sheet.GetRow(rowIndex);
                        if (row != null)
                        {
                            ExcelDataViewModel excelData = new ExcelDataViewModel
                            {
                                Column1 = row.GetCell(0)?.StringCellValue,
                                Column2 = row.GetCell(1)?.StringCellValue,
                                Column3 = row.GetCell(2)?.StringCellValue,// You can skip column 3, as it's not needed according to your requirement
                                Column6 = (double)(row.GetCell(5)?.NumericCellValue)
                            };
                            excelDataList.Add(excelData);
                        }
                    }

                    // Now you have the Excel data in excelDataList
                    // You can store it or process it further as needed

                    return RedirectToAction("Result");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while processing the file: " + ex.Message;
                return View();
            }
        }

        public IActionResult Result()
        {
            return View();
        }
    }
}
