using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MinCoreBank.Controllers.Auth
{
    public class AuthViewController : Controller
    {

        // Add to AuthViewController.cs

        [Authorize] // Allow any authenticated user (including limited access)
        public IActionResult ChangePassword()
        {
            // Check authentication
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "AuthView");
            }

            // Get user info from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity.Name;
            var mustChangePassword = User.FindFirst("MustChangePassword")?.Value;
            var limitedAccess = User.FindFirst("LimitedAccess")?.Value;

            // Debug info - remove in production
            ViewBag.DebugInfo = $"UserId: {userId}, Username: {userName}, MustChangePassword: {mustChangePassword}, LimitedAccess: {limitedAccess}";

            // Check if password change is required
            bool forceChange = mustChangePassword == "true" || mustChangePassword == "True";
            ViewBag.ForceChange = forceChange;
            ViewBag.UserId = userId;

            return View();
        }

        public IActionResult Login()
        {
            // If user is already authenticated, redirect to home
            if (User.Identity.IsAuthenticated)
            {
                var mustChangePassword = User.FindFirst("MustChangePassword")?.Value;
                var limitedAccess = User.FindFirst("LimitedAccess")?.Value;

                if (string.Equals(mustChangePassword, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(limitedAccess, "password-change-only", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("ChangePassword", "AuthView");
                }

                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // If user is not authenticated, redirect to login
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "AuthView");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutConfirmed()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    // Sign out from cookie authentication
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Clear session
                    HttpContext.Session.Clear();

                    // Clear authentication cookie manually
                    Response.Cookies.Delete("MinCoreBank.Auth");

                    // Ensure the cookie is expired
                    Response.Cookies.Append("MinCoreBank.Auth", "", new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    });
                }

                // Clear any cached data
                Response.Headers["Cache-Control"] = "no-cache, no-store";
                Response.Headers["Expires"] = "-1";
                Response.Headers["Pragma"] = "no-cache";

                return RedirectToAction("Login", "AuthView");
            }
            catch (Exception ex)
            {
                // Log exception here if you have logging
                // For now, redirect to login even if there's an error
                return RedirectToAction("Login", "AuthView");
            }
        }
    }
}
