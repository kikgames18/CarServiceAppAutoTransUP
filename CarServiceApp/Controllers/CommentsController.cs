using CarServiceApp.Data;
using CarServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServiceApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetInt32("UserId") != null;
        private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
        private string CurrentUserType() => HttpContext.Session.GetString("UserType");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int requestId, string message)
        {
            if (!IsAuthenticated())
                return Unauthorized();

            if (CurrentUserType() != "Автомеханик" && CurrentUserType() != "Менеджер")
                return Forbid();

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest();

            var comment = new Comment
            {
                Message = message,
                MasterID = CurrentUserId(),
                RequestID = requestId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Requests", new { id = requestId });
        }
    }
}