using CarServiceApp.Data;
using CarServiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServiceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == login && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserID);
                HttpContext.Session.SetString("UserType", user.Type);
                HttpContext.Session.SetString("UserFIO", user.FIO);
                return RedirectToAction("Index", "Requests");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}