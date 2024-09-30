using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ST10251759_CLDV6212_POE_Part_1.Models;
using ST10251759_CLDV6212_POE_Part_1.Services;
using System.Security.Claims;

/*

 ==============================Code Attribution==================================

 Author: w3schools
 Link: https://www.w3schools.com/html/
 Date Accessed: 16 August 2024

 Author: HTML Codex
 Link:https://htmlcodex.com/
 Date Accessed: 16 August 2024

 Author: w3schools
 Link: https://www.w3schools.com/css/
 Date Accessed: 16 August 2024

 Author: w3schools
 Link: https://www.w3schools.com/js/
 Date Accessed: 16 August 2024

 Author: Mick Gouweloos
 Link: https://github.com/mickymouse777/Cloud_Storage/tree/master/Cloud_Storage
 Date Accessed: 16 August 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview?pivots=programming-language-csharp
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/service-connector/quickstart-portal-functions-connection?tabs=SMI
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob?tabs=isolated-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp 
 Date Accessed: 25 September 2024

 Author: Microsoft
 Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=isolated-process%2Ctable-api%2Cextensionv3&pivots=programming-language-csharp
 Date Accessed: 25 September 2024


 *********All Images used throughout project are adapted from Pinterest (https://www.furiousmotorsport.com/)*************

 ==============================Code Attribution==================================
     
 */

namespace ST10251759_CLDV6212_POE_Part_1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        //GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
          

            if (ModelState.IsValid)
            {
                bool success = await _authService.RegisterAsync(model.Email, model.Password, model.FullName);
                if (success)
                    return RedirectToAction("Login");
                ModelState.AddModelError("", "User already exists.");
            }



            return View(model);
        }

        //GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _authService.LoginAsync(model.Email, model.Password);
                if (user != null)
                {
                    //Create claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        //Configure as needed ***
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                    );

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }

        //POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

    }
}
