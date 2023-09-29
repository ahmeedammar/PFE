using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            // Code to handle the settings page logic
            // Return the Settings view
            return View();
        }

        
        [HttpPost]
        public IActionResult SaveSettings(SettingsModel model)
        {
            var selectedSidebarOrder = model.SidebarOrder;
            // Code to save the selected sidebar order
            // Redirect to the Settings page or another appropriate page
            return RedirectToAction("Index");
        }

    }
}
