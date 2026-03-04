using CarServiceApp.Models;
using CarServiceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarServiceApp.Controllers
{
    public class RequestsController : Controller
    {
        private readonly DataService _dataService;

        public RequestsController(DataService dataService)
        {
            _dataService = dataService;
        }

        // Проверка аутентификации
        private bool IsAuthenticated() => HttpContext.Session.GetInt32("UserId") != null;
        private string CurrentUserType() => HttpContext.Session.GetString("UserType");
        private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        // GET: /Requests
        public IActionResult Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var requests = _dataService.Requests.AsEnumerable();

            // Если пользователь - заказчик, показываем только его заявки
            if (CurrentUserType() == "Заказчик")
                requests = requests.Where(r => r.ClientID == CurrentUserId());

            return View(requests.ToList());
        }

        // GET: /Requests/Edit/5 (если id=0 – новая заявка)
        public IActionResult Edit(int? id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            Request request;
            if (id == null || id == 0)
            {
                // Новая заявка
                request = new Request
                {
                    StartDate = DateTime.Today,
                    RequestStatus = "Новая заявка"
                };
            }
            else
            {
                request = _dataService.Requests.FirstOrDefault(r => r.RequestID == id);
                if (request == null)
                    return NotFound();
            }

            ViewBag.Clients = _dataService.Users.Where(u => u.Type == "Заказчик").ToList();
            ViewBag.Masters = _dataService.Users.Where(u => u.Type == "Автомеханик").ToList();
            ViewBag.CurrentUserType = CurrentUserType();
            ViewBag.CurrentUserId = CurrentUserId();

            return View(request);
        }

        // POST: /Requests/Edit
        [HttpPost]
        public IActionResult Edit(Request request)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            // Для новой заявки
            if (request.RequestID == 0)
            {
                request.RequestID = _dataService.GetNextRequestId();
                _dataService.Requests.Add(request);
            }
            else
            {
                var existing = _dataService.Requests.FirstOrDefault(r => r.RequestID == request.RequestID);
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
                // StartDate обычно не меняется, но можно разрешить
                existing.StartDate = request.StartDate;
            }

            return RedirectToAction("Index");
        }

        // POST: /Requests/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsAuthenticated() || (CurrentUserType() != "Оператор" && CurrentUserType() != "Менеджер"))
                return Forbid();

            var request = _dataService.Requests.FirstOrDefault(r => r.RequestID == id);
            if (request != null)
            {
                _dataService.Requests.Remove(request);
                // Удаляем связанные комментарии
                _dataService.Comments.RemoveAll(c => c.RequestID == id);
            }

            return RedirectToAction("Index");
        }

        // GET: /Requests/Details/5
        public IActionResult Details(int id)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Account");

            var request = _dataService.Requests.FirstOrDefault(r => r.RequestID == id);
            if (request == null)
                return NotFound();

            var comments = _dataService.Comments.Where(c => c.RequestID == id).ToList();
            ViewBag.Comments = comments;
            ViewBag.CurrentUserType = CurrentUserType();
            ViewBag.CurrentUserId = CurrentUserId();

            return View(request);
        }
    }
}