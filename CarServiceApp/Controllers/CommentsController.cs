using CarServiceApp.Models;
using CarServiceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarServiceApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly DataService _dataService;

        public CommentsController(DataService dataService)
        {
            _dataService = dataService;
        }

        private bool IsAuthenticated() => HttpContext.Session.GetInt32("UserId") != null;
        private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
        private string CurrentUserType() => HttpContext.Session.GetString("UserType");

        // POST: /Comments/Add
        [HttpPost]
        public IActionResult Add(int requestId, string message)
        {
            if (!IsAuthenticated())
                return Unauthorized();

            // Только механик или менеджер могут добавлять комментарии
            if (CurrentUserType() != "Автомеханик" && CurrentUserType() != "Менеджер")
                return Forbid();

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest();

            var comment = new Comment
            {
                CommentID = _dataService.GetNextCommentId(),
                Message = message,
                MasterID = CurrentUserId(),
                RequestID = requestId
            };

            _dataService.Comments.Add(comment);

            // Возвращаемся на страницу деталей заявки
            return RedirectToAction("Details", "Requests", new { id = requestId });
        }
    }
}