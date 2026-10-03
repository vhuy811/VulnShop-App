using Microsoft.AspNetCore.Mvc;

namespace VulnShop.Controllers;

// ===== G3.4 | IDOR/BOLA (CWE-639): kiem soat truy cap theo doi tuong =====
// Dang nhap bang cookie (sid) voi hai nguoi dung that: alice, bob.
// Moi nguoi "so huu" mot don hang. /Account/Don KHONG kiem quyen so huu nen
// nguoi nay doc duoc don cua nguoi kia -> IDOR.
//
// ZAP active scan thuong KHONG bat duoc loi nay (no khong co khai niem "tai
// nguyen nay cua ai"). Bo kiem DAST CO XAC THUC (tools/idor.py, cau hinh o
// .devsecops/idor.json) dang nhap bang ca hai nguoi roi thu truy cap cheo ->
// bat duoc, phat thanh cong cu RIENG 'DAST-idor' (muc High, cong CHAN).
//
// Day la PR demo: cong IDOR phai chan code hong TRUOC KHI vao main (giong kich
// ban C1-C4 cua ProductController). Main giu nguyen trang thai sach.
public class AccountController : Controller
{
    // Kho nguoi dung toi gian cho demo - KHONG phai cach lam that.
    private static readonly Dictionary<string, string> MatKhau = new()
    {
        ["alice"] = "alice123",
        ["bob"] = "bob123",
    };

    // Don hang: id -> (chu so huu, mo ta). alice so huu don 1, bob so huu don 2.
    private static readonly Dictionary<int, (string Chu, string MoTa)> DonHang = new()
    {
        [1] = ("alice", "Ban phim co - 1.200.000d"),
        [2] = ("bob", "Man hinh 27 inch - 5.600.000d"),
    };

    // Trang dang nhap (GET): form tinh dung nhay don cho thuoc tinh HTML.
    [HttpGet]
    public IActionResult Login()
    {
        const string html =
            "<!doctype html><meta charset='utf-8'><title>Dang nhap</title>" +
            "<h2>Dang nhap VulnShop</h2>" +
            "<form method='post' action='/Account/Login'>" +
            "<p>Tai khoan: <input name='username'></p>" +
            "<p>Mat khau: <input type='password' name='password'></p>" +
            "<button type='submit'>Dang nhap</button></form>";
        return Content(html, "text/html; charset=utf-8");
    }

    // Dang nhap (POST): dung -> dat cookie sid roi 302; sai -> 401.
    [HttpPost]
    public IActionResult Login(string? username, string? password)
    {
        if (username != null && MatKhau.TryGetValue(username, out var mk) && mk == password)
        {
            // Cookie phien toi gian: sid = ten dang nhap (du cho demo IDOR).
            Response.Cookies.Append("sid", username,
                new CookieOptions { HttpOnly = true, Path = "/" });
            return Redirect("/");
        }
        return Unauthorized();
    }

    // LO IDOR/BOLA: chi can DANG NHAP la doc duoc BAT KY don hang nao theo id,
    // khong he kiem tra don do co thuoc ve nguoi dang dang nhap hay khong.
    [HttpGet]
    public IActionResult Don(int id)
    {
        var ai = Request.Cookies["sid"];
        if (string.IsNullOrEmpty(ai)) return Unauthorized();
        if (!DonHang.TryGetValue(id, out var d)) return NotFound();
        return Content($"Don #{id} | Chu: {d.Chu} | {d.MoTa}", "text/plain; charset=utf-8");
    }
}
