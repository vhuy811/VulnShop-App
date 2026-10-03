using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Text.Encodings.Web;
using VulnShop.Data;

namespace VulnShop.Controllers;

public class ProductController : Controller
{
    // ===== C1 | da va: tham so hoa @q =====
    public IActionResult Search(string q)
    {
        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Category, Price FROM Products WHERE Name LIKE @q";
        cmd.Parameters.AddWithValue("@q", "%" + (q ?? "") + "%");
        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                rows.Add($"{reader.GetValue(0)} | {reader.GetValue(1)} | {reader.GetValue(2)} | {reader.GetValue(3)}");
        }
        catch (SqliteException ex)
        {
            rows.Add("SQL error: " + ex.Message);
        }
        ViewBag.Query = q;
        return View(rows);
    }

    // ===== C2 | da va: tham so hoa @id =====
    public IActionResult Detail(string id)
    {
        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Category, Price FROM Products WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id ?? "");
        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                rows.Add($"{reader.GetValue(0)} | {reader.GetValue(1)} | {reader.GetValue(2)} | {reader.GetValue(3)}");
        }
        catch (SqliteException ex)
        {
            rows.Add("SQL error: " + ex.Message);
        }
        ViewBag.Query = id;
        return View("Search", rows);
    }

    // ===== C3 | da va: view dung @ViewBag.Msg, Razor tu ma hoa HTML =====
    public IActionResult Echo(string msg)
    {
        ViewBag.Msg = msg;
        return View();
    }

    // ===== C4 | da va: tra view, Razor tu ma hoa HTML =====
    public IActionResult Greet(string name)
    {
        ViewBag.Name = name;
        return View();
    }

    // ===== C5 | an toan tu dau: WHERE da tham so hoa =====
    public IActionResult SafeSearch(string q)
    {
        var sortColumn = "Name";
        var sql = "SELECT Id, Name, Category, Price FROM Products WHERE Name LIKE @q ORDER BY " + sortColumn;

        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@q", "%" + (q ?? "") + "%");
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            rows.Add($"{reader.GetValue(0)} | {reader.GetValue(1)} | {reader.GetValue(2)} | {reader.GetValue(3)}");

        ViewBag.Query = q;
        return View("Search", rows);
    }

    // ===== C6 | an toan tu dau: da HtmlEncode truoc khi noi vao chuoi HTML =====
    public IActionResult SafeGreet(string name)
    {
        var safe = HtmlEncoder.Default.Encode(name ?? "");
        var html = "<h3>Xin chao " + safe + "</h3><p>Chuc ban mua sam vui ve.</p>";
        return Content(html, "text/html");
    }

    // Loc san pham theo danh muc
    public IActionResult Filter(string category)
    {
        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Category, Price FROM Products WHERE Category = @category";
        cmd.Parameters.AddWithValue("@category", category ?? "");
        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                rows.Add($"{reader.GetValue(0)} | {reader.GetValue(1)} | {reader.GetValue(2)} | {reader.GetValue(3)}");
        }
        catch (SqliteException ex)
        {
            rows.Add("SQL error: " + ex.Message);
        }
        ViewBag.Query = category;
        return View("Search", rows);
    }

    // Tim nhanh theo tu khoa
    public IActionResult TimNhanh(string tu)
    {
        ViewBag.Query = tu;
        return View("Search", DocSanPham("Name LIKE @tu", "@tu", "%" + (tu ?? "") + "%"));
    }

    private static List<string> DocSanPham(string dieuKien, string tenThamSo, string giaTri)
    {
        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        var cauLenh = "SELECT Id, Name, Category, Price FROM Products WHERE " + dieuKien;
        cmd.CommandText = cauLenh;
        cmd.Parameters.AddWithValue(tenThamSo, giaTri);
        try
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                rows.Add($"{reader.GetValue(0)} | {reader.GetValue(1)} | {reader.GetValue(2)} | {reader.GetValue(3)}");
        }
        catch (SqliteException ex)
        {
            rows.Add("SQL error: " + ex.Message);
        }
        return rows;
    }

    // ===== Cac endpoint vo hai =====
    public IActionResult List()
    {
        var rows = new List<string>();
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Category, Price FROM Products ORDER BY Id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            rows.Add($"{reader.GetValue(0)} | {reader.GetValue(1)} | {reader.GetValue(2)} | {reader.GetValue(3)}");

        ViewBag.Query = "(tat ca)";
        return View("Search", rows);
    }

    public IActionResult About() => Content("<h3>Gioi thieu VulnShop</h3>", "text/html");

    public IActionResult Contact() => Content("<h3>Lien he: support@vulnshop.local</h3>", "text/html");

    public IActionResult Help() => Content("<h3>Trung tam tro giup</h3>", "text/html");

    public IActionResult Pricing() => Content("<h3>Bang gia va chinh sach giao hang</h3>", "text/html");

    // ===== TEST 1 | SACH: dem san pham theo danh muc (tham so hoa @category) =====
    public IActionResult CountByCategory(string category)
    {
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Products WHERE Category = @category";
        cmd.Parameters.AddWithValue("@category", category ?? "");
        var n = cmd.ExecuteScalar();
        return Content($"So san pham: {n}", "text/plain");
    }
}
