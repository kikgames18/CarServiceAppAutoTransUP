using CarServiceApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServiceApp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetInt32("UserId") != null;

        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var completed = await _context.Requests
                .Where(r => r.RequestStatus == "Готова к выдаче" && r.CompletionDate != null)
                .ToListAsync();

            int completedCount = completed.Count;
            double? avgTime = null;

            if (completedCount > 0)
            {
                avgTime = completed.Average(r => (r.CompletionDate.Value - r.StartDate).TotalDays);
            }

            var problemStats = await _context.Requests
                .GroupBy(r => r.ProblemDescription)
                .Select(g => new { Problem = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.CompletedCount = completedCount;
            ViewBag.AvgTime = avgTime.HasValue ? avgTime.Value.ToString("F1") + " дн." : "нет данных";
            ViewBag.ProblemStats = problemStats;

            return View();
        }
    }
}