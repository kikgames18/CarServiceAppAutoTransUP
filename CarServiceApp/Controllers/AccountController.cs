using CarServiceApp.Models;
using CarServiceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarServiceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataService _dataService;

        public AccountController(DataService dataService)
        {
            _dataService = dataService;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string login, string password)
        {
            var user = _dataService.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                // Сохраняем данные пользователя в сессии
                HttpContext.Session.SetInt32("UserId", user.UserID);
                HttpContext.Session.SetString("UserType", user.Type);
                HttpContext.Session.SetString("UserFIO", user.FIO);

                return RedirectToAction("Index", "Requests");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}