using Microsoft.AspNetCore.Mvc;

namespace ST10251759_CLDV6212_POE_Part_1.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AccessDenied()
        {
            // PASSING USER LOGGED IN
            if (User.Identity.Name != null)
            {
                ViewBag.IsLoggedIn = true;
                var userName = User.Identity.Name;
                ViewBag.UserName = userName;

            }
            else
            {
                ViewBag.UserRole = "Guest";
                ViewBag.IsLoggedIn = false;
                
            }

            return View();
        }

        public IActionResult NeedToLogin()
        {
            // PASSING USER LOGGED IN
            if (User.Identity.Name != null)
            {
                ViewBag.IsLoggedIn = true;
                var userName = User.Identity.Name;
                ViewBag.UserName = userName;

            }
            else
            {
                ViewBag.UserRole = "Guest";
                ViewBag.IsLoggedIn = false;
                
            }

            return View();
        }
    }
}
