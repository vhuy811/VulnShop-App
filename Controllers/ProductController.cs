using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Text.Encodings.Web;
using VulnShop.Data;

namespace VulnShop.Controllers;

// Ban tren tag `ground-truth` co bon lo hong gieo co y (C1-C4) de danh gia
// pipeline; ket qua doi chieu nam trong ground_truth.csv.
//
// Ban tren main nay DA VA ca bon. Day la diem xuat phat SACH cho kich ban
// lam viec nhom: cong phai chan duoc code hong TRUOC KHI no vao main.
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
    // Van noi chuoi nhung chi noi ten cot cung, khong lay tu input.
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

    // Tim nhanh theo tu khoa: dieu kien loc la hang so, gia tri nguoi dung di qua tham so @tu
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

    // ===== Cac endpoint vo hai, dung de tang be mat tan cong =====
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

    // ===== TEST 3 | LO IDOR/BOLA (CWE-639): tra ve Secret cua BAT KY user theo id,
    // khong kiem quyen so huu. SQL da tham so hoa -> SAST khong bao SQLi; ZAP khong
    // co active-scan cho IDOR; main chua co .devsecops/idor.json -> DAST-idor rong.
    // Ky vong: khong cong nao chan -> MERGE DUOC du lo secret (bypass).
    public IActionResult UserSecret(string id)
    {
        string result = "(khong thay)";
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, Secret FROM Users WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id ?? "");
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            result = $"User #{reader.GetValue(0)} ({reader.GetValue(1)}) secret = {reader.GetValue(2)}";
        return Content(result, "text/plain");
    }
}
