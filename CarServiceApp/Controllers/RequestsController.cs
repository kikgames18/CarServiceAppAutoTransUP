using CarServiceApp.Data;
using CarServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServiceApp.Controllers
{
    public class RequestsController : Controller
    {
        private readonly AppDbContext _context;

        public RequestsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetInt32("UserId") != null;
        private string CurrentUserType() => HttpContext.Session.GetString("UserType");
        private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        // GET: /Requests
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            IQueryable<Request> requests = _context.Requests
                .Include(r => r.Client)
                .Include(r => r.Master);

            if (CurrentUserType() == "Заказчик")
                requests = requests.Where(r => r.ClientID == CurrentUserId());

            return View(await requests.ToListAsync());
        }

        // GET: /Requests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            Request request;
            if (id == null || id == 0)
            {
                request = new Request
                {
                    StartDate = DateTime.Today,
                    RequestStatus = "Новая заявка"
                };
            }
            else
            {
                request = await _context.Requests.FindAsync(id);
                if (request == null)
                    return NotFound();
            }

            ViewBag.Clients = await _context.Users.Where(u => u.Type == "Заказчик").ToListAsync();
            ViewBag.Masters = await _context.Users.Where(u => u.Type == "Автомеханик").ToListAsync();
            ViewBag.CurrentUserType = CurrentUserType();
            ViewBag.CurrentUserId = CurrentUserId();

            return View(request);
        }

        // POST: /Requests/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Request request)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            if (request.RequestID == 0)
            {
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Requests.FindAsync(request.RequestID);
                if (existing == null)
                    return NotFound();

                existing.CarType = request.CarType;
                existing.CarModel = request.CarModel;
                existing.ProblemDescription = request.ProblemDescription;
                existing.RequestStatus = request.RequestStatus;
                existing.CompletionDate = request.CompletionDate;
                existing.RepairParts = request.RepairParts;
                existing.MasterID = request.MasterID;
                existing.ClientID = request.ClientID;
                existing.StartDate = request.StartDate;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST: /Requests/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated() || (CurrentUserType() != "Оператор" && CurrentUserType() != "Менеджер"))
                return Forbid();

            var request = await _context.Requests.FindAsync(id);
            if (request != null)
            {
                _context.Requests.Remove(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // GET: /Requests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var request = await _context.Requests
                .Include(r => r.Client)
                .Include(r => r.Master)
                .FirstOrDefaultAsync(r => r.RequestID == id);

            if (request == null)
                return NotFound();

            var comments = await _context.Comments
                .Where(c => c.RequestID == id)
                .Include(c => c.Master)
                .ToListAsync();

            ViewBag.Comments = comments;
            ViewBag.CurrentUserType = CurrentUserType();
            ViewBag.CurrentUserId = CurrentUserId();

            return View(request);
        }
    }
}