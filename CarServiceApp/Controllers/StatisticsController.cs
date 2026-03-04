using CarServiceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarServiceApp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly DataService _dataService;

        public StatisticsController(DataService dataService)
        {
            _dataService = dataService;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetInt32("UserId") != null;

        // GET: /Statistics
        public IActionResult Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            // Завершённые заявки (статус "Готова к выдаче" и дата завершения не null)
            var completed = _dataService.Requests
                .Where(r => r.RequestStatus == "Готова к выдаче" && r.CompletionDate.HasValue)
                .ToList();

            int completedCount = completed.Count;
            double? avgTime = null;

            if (completedCount > 0)
            {
                avgTime = completed.Average(r => (r.CompletionDate.Value - r.StartDate).TotalDays);
            }

            // Группировка по описанию проблемы
            var problemStats = _dataService.Requests
                .GroupBy(r => r.ProblemDescription)
                .Select(g => new { Problem = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.CompletedCount = completedCount;
            ViewBag.AvgTime = avgTime.HasValue ? avgTime.Value.ToString("F1") + " дн." : "нет данных";
            ViewBag.ProblemStats = problemStats;

            return View();
        }
    }
}