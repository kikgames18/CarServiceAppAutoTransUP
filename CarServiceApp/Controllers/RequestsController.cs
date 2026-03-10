using CarServiceApp.Data;
using CarServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder; // Не забудьте добавить using

namespace CarServiceApp.Controllers
{
    public class RequestsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _feedbackFormUrl = "https://docs.google.com/forms/d/e/1FAIpQLSdhZcExx6LSIXxk0ub55mSu-WIh23WYdGG9HY5EZhLDo7P8eA/viewform?usp=sf_link";

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
            ViewBag.Masters = await _context.Users.Where(u => u.Type == "Автомеханик" || u.Type == "Менеджер по качеству").ToListAsync(); // менеджер по качеству тоже может быть назначен?
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
                // Новая заявка
                _context.Requests.Add(request);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.Requests.FindAsync(request.RequestID);
                if (existing == null)
                    return NotFound();

                // Общие поля (доступны всем)
                existing.CarType = request.CarType;
                existing.CarModel = request.CarModel;
                existing.ProblemDescription = request.ProblemDescription;
                existing.RequestStatus = request.RequestStatus;
                existing.CompletionDate = request.CompletionDate;
                existing.RepairParts = request.RepairParts;

                // Поля, доступные только менеджеру по качеству (и обычному менеджеру)
                if (CurrentUserType() == "Менеджер по качеству" || CurrentUserType() == "Менеджер")
                {
                    existing.MasterID = request.MasterID;
                    existing.ExtendedDeadline = request.ExtendedDeadline;
                    existing.DeadlineAgreed = request.DeadlineAgreed;
                }

                // Можно разрешить изменение клиента только определённым ролям
                // existing.ClientID = request.ClientID;

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

        // GET: /Requests/QrCode/5 (возвращает изображение QR-кода)
        public IActionResult QrCode(int id)
        {
            // Можно использовать id для создания уникальной ссылки, если нужно
            // Например, можно добавить параметр ?requestId=id к URL формы.
            // Но в задании ссылка фиксированная.
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(_feedbackFormUrl, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                return File(qrCodeImage, "image/png");
            }
        }
    }
}