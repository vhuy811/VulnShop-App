using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using VulnShop.Data;

namespace VulnShop.Controllers;

// === NHANH TRINH DIEN - GIEO LOI CO Y, KHONG MERGE ===
// Trang bao cao noi bo cho quan tri vien. Moi lo hong duoi day deu co chu dich
// va nham vao mot tang khac nhau cua pipeline.
public class AdminReportController : Controller
{
    // LO HONG 1 (CWE-798) - khoa dich vu bao cao viet thang vao ma nguon.
    // Tang bat duoc: gitleaks (T0).
    private const string ReportingApiKey = "AKIA2E0VQ7X9MZPLR4TB";

    // LO HONG 2 (CWE-89) - tham so region noi thang vao cau lenh SQL.
    // Tang bat duoc: Semgrep rule du an + rule cong dong + CodeQL (T2),
    // va OWASP-ZAP khai thac that luc chay (T5).
    public IActionResult Sales(string region = "Keyboards")
    {
        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Name, Category, Price FROM Products WHERE Category = '" + region + "'";
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            rows.Add($"{r.GetString(0)} | {r.GetString(1)} | {r.GetDouble(2):n0}");
        }
        ViewBag.Region = region;
        ViewBag.Rows = rows;
        return View();
    }

    // LO HONG 3 (CWE-79) - dung chuoi nguoi dung gui len thanh HTML roi tra ve.
    // Tang bat duoc: Semgrep (T2) va OWASP-ZAP (T5).
    public IActionResult Note(string? text)
    {
        var html = "<div class='card'><div class='card-body'>" + text + "</div></div>";
        return Content(html, "text/html");
    }

    // LO HONG 4 (CWE-639 - IDOR/BOLA) - tra ve don hang theo id ma KHONG kiem
    // tra don hang do co phai cua nguoi dang dang nhap khong.
    // KHONG tang tinh nao bat duoc: "don hang nay cua ai" la rang buoc nghiep
    // vu, khong phai mau ma. Chi lop DAST-idor (T5) bat.
    public IActionResult Invoice(int orderId)
    {
        var order = Db.GetOrder(orderId);
        if (order == null) return NotFound();
        return Content($"Hoa don #{order.Id} - {order.CustomerName} - {order.Total:n0} d",
                       "text/plain");
    }

    // LO HONG 5 (CWE-601) - chuyen huong toi dia chi do nguoi dung quyet dinh.
    // Tang bat duoc: Semgrep (T2, muc chi bao) va OWASP-ZAP (T5, muc High).
    public IActionResult Go(string nextUrl)
    {
        return Redirect(nextUrl);
    }

    // LO HONG 6 (CWE-22) - ghep ten tep nguoi dung gui vao duong dan.
    // Tang bat duoc: Semgrep (T2).
    public IActionResult Export(string name)
    {
        return Content(ReportFiles.Read(name), "text/plain");
    }

    // LO HONG 7 (CWE-328) - bam MD5 lam dau xac thuc bao cao.
    // LO HONG 8 (CWE-321) - khoa HMAC gan cung trong ma nguon.
    // Tang bat duoc: Semgrep (T2), hai rule khac nhau.
    public IActionResult Sign(string data)
    {
        var digest = MD5.HashData(Encoding.UTF8.GetBytes(data + ReportingApiKey));

        var key = "bao-cao-doanh-thu-2026-khoa-ky";
        using var mac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var tag = mac.ComputeHash(digest);

        return Content(Convert.ToHexString(tag), "text/plain");
    }
}

// Doc tep bao cao da xuat. Nam ngoai Controller de 'File' tro dung
// System.IO.File chu khong phai ham File() cua ControllerBase.
internal static class ReportFiles
{
    public static string Read(string name) => File.ReadAllText("exports/" + name);
}
