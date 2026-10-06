using Microsoft.AspNetCore.Mvc;

namespace VulnShop.Controllers;

// Minimal cookie-based sign-in for the demo shop.
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // SECURITY REGRESSION TEST ONLY: anti-forgery protection removed on purpose
    // so CodeQL re-reports CWE-352 and the authenticated DAST-idor probe can
    // sign in to exercise the IDOR endpoint. Reverted before merge.
    [HttpPost]
    public IActionResult Login(string username, string password, string? returnUrl)
    {
        string? sid = username switch
        {
            "alice" => "alice",
            "bob" => "bob",
            _ => null,
        };
        if (sid is null)
        {
            ViewBag.Error = "Invalid account. Try alice or bob.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        Response.Cookies.Append("sid", sid, new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
        });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("MyOrders", "Order");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("sid");
        return RedirectToAction("Index", "Home");
    }
}
