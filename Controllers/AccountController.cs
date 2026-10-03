using Microsoft.AspNetCore.Mvc;

namespace VulnShop.Controllers;

// ================================================================
//  DEMO DAST-IDOR  —  minh hoa khi nao DAST phat huy ro nhat.
//
//  Tinh nang: dang nhap bang cookie (alice / bob) + xem don hang theo id.
//  Lo DUY NHAT loi IDOR/BOLA (CWE-639): action Don() chi kiem tra DA DANG
//  NHAP, KHONG kiem tra don hang co thuoc ve nguoi dang dang nhap hay khong.
//
//  Co y KHONG kem bat ky lo hong nao khac ma SAST bat duoc:
//   - Khong dung SQL (don hang la du lieu gia lap trong bo nho) -> khong SQLi.
//   - Gia tri cookie 'sid' lay tu HANG (literal) qua switch, KHONG phai tu
//     chuoi nhap vao -> khong dinh loi chen header (CWE-113).
//  Nho vay Semgrep / CodeQL / ZAP deu "xanh", chi DAST-idor bat duoc loi nay.
// ================================================================
public class AccountController : Controller
{
    // Don hang gia lap: id -> (chu so huu, noi dung). Cung mot id luon tra ve
    // cung noi dung -> lop DAST-idor so sanh body giua hai nguoi dung duoc.
    private static readonly Dictionary<int, (string chu, string noiDung)> Orders = new()
    {
        [1] = ("alice", "Don #1 (alice): Ban phim co - 1.200.000d - giao 12 Hai Ba Trung"),
        [2] = ("bob",   "Don #2 (bob): Tai nghe chong on - 2.300.000d - giao 48 Le Loi"),
    };

    // POST /Account/Login (form: username, password) -> dat cookie phien.
    public IActionResult Login(string username, string password)
    {
        // Chi alice/bob dang nhap duoc. sid gan tu HANG -> khong nhiem ban.
        string? sid = username switch
        {
            "alice" => "alice",
            "bob" => "bob",
            _ => null,
        };
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

    // GET /Account/Don?id=... -> xem noi dung don hang theo id.
    [HttpGet]
    public IActionResult Don(int id)
    {
        var ai = Request.Cookies["sid"];
        if (string.IsNullOrEmpty(ai))
            return Unauthorized();

        if (!Orders.TryGetValue(id, out var don))
            return NotFound();

        // ===== LO IDOR/BOLA (CWE-639) =====
        // Thieu dong kiem tra quyen so huu, vi du:
        //     if (don.chu != ai) return Forbid();
        // -> bob chi can doi id=1 la doc duoc don cua alice.
        return Content(don.noiDung, "text/plain; charset=utf-8");
    }
}
