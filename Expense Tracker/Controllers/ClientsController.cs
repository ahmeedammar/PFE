using Expense_Tracker.Filters;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_Tracker.Controllers
{
    [AuthorizeAdmin]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var Clients = await _context.Clients.ToListAsync();
            return View(Clients);
        }

        // GET: Clients/AddOrEdit/5
        public async Task<IActionResult> AddOrEdit(int id)
        {
            if (id == 0)
            {
                return View("AddOrEdit", new Client());
            }
            else
            {
                var Client = await _context.Clients.FindAsync(id);
                if (Client == null)
                {
                    return NotFound();
                }
                return View("AddOrEdit", Client);
            }
        }

        // POST: Clients/AddOrEdit/5
        // POST: Clients/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, [Bind("ClientId,Name")] Client client)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == 0)
                    {
                        _context.Add(client);
                    }
                    else
                    {
                        var existingClient = await _context.Clients.FindAsync(id);
                        if (existingClient == null)
                        {
                            return NotFound();
                        }

                        existingClient.Name = client.Name;
                        _context.Update(existingClient);
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Clients"); // Redirect to Clients list
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Clients");
        }


        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}
