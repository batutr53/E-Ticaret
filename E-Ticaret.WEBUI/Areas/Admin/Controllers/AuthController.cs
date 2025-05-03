using E_Ticaret.Data;
using E_Ticaret.Service.Helpers;
using E_Ticaret.WEBUI.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(DatabaseContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.AppUsers.FirstOrDefault(x => x.UserName == username && x.Password == password && x.IsActive);

            if (user == null)
            {
                ViewBag.LoginError = "E-posta veya şifre hatalı!";
                return View();
            }

            var token = _jwtHelper.GenerateToken(user);

            Response.Cookies.Append("jwt", token, JwtCookieOptionsHelper.CreateWriteOptions());


            return RedirectToAction("Index", "Main", new { area = "Admin" });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Append("jwt", "", JwtCookieOptionsHelper.CreateDeleteOptions());


            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }
    }

}
