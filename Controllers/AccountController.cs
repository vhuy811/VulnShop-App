using Microsoft.AspNetCore.Mvc;

namespace VulnShop.Controllers;

// ================================================================
//  BAN DA VA (dung cho nhanh main) — co kiem tra quyen so huu.
//  Giong ban demo nhung Don() CO dong `if (don.chu != ai) return Forbid();`
//  -> khong con IDOR. Dua ban nay + .devsecops/idor.json len main thi moi PR
//  ve sau deu duoc kiem IDOR; PR nao lam mat dong kiem tra quyen so huu se bi
//  DAST-idor chan.
// ================================================================
public class AccountController : Controller
{
    private static readonly Dictionary<int, (string chu, string noiDung)> Orders = new()
    {
        [1] = ("alice", "Don #1 (alice): Ban phim co - 1.200.000d - giao 12 Hai Ba Trung"),
        [2] = ("bob",   "Don #2 (bob): Tai nghe chong on - 2.300.000d - giao 48 Le Loi"),
    };

    public IActionResult Login(string username, string password)
    {
        string? sid = username switch { "alice" => "alice", "bob" => "bob", _ => null };
        if (sid is null)
            return Unauthorized();
        Response.Cookies.Append("sid", sid, new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
        });
        return Content($"Da dang nhap: {sid}", "text/plain; charset=utf-8");
    }

    [HttpGet]
    public IActionResult Don(int id)
    {
        var ai = Request.Cookies["sid"];
        if (string.IsNullOrEmpty(ai))
            return Unauthorized();

        if (!Orders.TryGetValue(id, out var don))
            return NotFound();

        // ===== DA VA: kiem tra quyen so huu truoc khi tra ve =====
        if (don.chu != ai)
            return StatusCode(403, "Khong co quyen xem don hang nay.");

        return Content(don.noiDung, "text/plain; charset=utf-8");
    }
}
