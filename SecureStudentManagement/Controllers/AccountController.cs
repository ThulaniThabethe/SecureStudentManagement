using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace SecureStudentManagement.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return User?.Identity?.IsAuthenticated == true
                ? RedirectToAction("List", "Student")
                : View();
        }

        [HttpPost]
        public IActionResult StartLogin(string returnUrl = "/Student/List")
        {
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect(returnUrl);

            var authSettings = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(authSettings, "GitHubSecureOAuth");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return User.Identity.IsAuthenticated
                ? View()
                : RedirectToAction(nameof(Login));
        }
    }
}
