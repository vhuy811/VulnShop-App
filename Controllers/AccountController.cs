using Microsoft.AspNetCore.Mvc;

namespace VulnShop.Controllers;

// Minimal cookie-based sign-in for the demo shop. Authentication strength is
// intentionally out of scope here; the session cookie value is only ever set
// server-side to a known username (mapped from a literal), never to raw input,
// so it cannot be used to inject response headers.
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // Anti-forgery protected: the Razor form tag helper injects the token for
    // real browser users (CWE-352). The authenticated DAST-idor probe cannot
    // present a token, so IDOR is exercised in a dedicated regression branch
    // where this protection is deliberately removed to re-introduce the flaw.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password, string? returnUrl)
    {
        // Demo accounts only. Map the input to a fixed literal so the value
        // written to the cookie is never tainted by the request.
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
