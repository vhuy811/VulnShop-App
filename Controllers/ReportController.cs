using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using VulnShop.Data;

namespace VulnShop.Controllers;

// SECURITY REGRESSION TEST ONLY - intentionally vulnerable.
// Added to prove the DevSecOps gates catch brand-new findings:
//   - SQL injection  (SAST: Semgrep taint + CodeQL)        -> Lookup
//   - Reflected XSS  (SAST: CodeQL; DAST: OWASP ZAP)        -> Note
//   - IDOR / BOLA    (DAST: DAST-idor authenticated probe)  -> Invoice
//   - Insecure cookie(SAST: CodeQL; DAST: DAST-runtime)     -> Invoice
// This file is reverted before the branch is merged.
public class ReportController : Controller
{
    // SQL injection: 'sku' (request input) is concatenated into the SQL text.
    // Source = query parameter, sink = SqliteCommand.CommandText.
    [HttpGet]
    public IActionResult Lookup(string sku)
    {
        using var conn = new SqliteConnection(Db.ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM Products WHERE Name = '" + sku + "'";
        var rows = new List<string>();
        using var r = cmd.ExecuteReader();
        while (r.Read()) rows.Add(r.GetInt32(0) + ": " + r.GetString(1));
        return Content("Matches: " + string.Join(", ", rows));
    }

    // Reflected XSS: user input echoed into an HTML response with no encoding.
    [HttpGet]
    public IActionResult Note(string text)
    {
        return Content(
            "<html><body><h3>Your note</h3><div>" + text + "</div></body></html>",
            "text/html");
    }

    // IDOR / BOLA: returns any order by id with no ownership check.
    [HttpGet]
    public IActionResult Invoice(int id)
    {
        // Insecure tracking cookie: no Secure, no HttpOnly (runtime finding).
        Response.Cookies.Append("vs_invoice_seen", id.ToString(),
            new CookieOptions { Secure = false, HttpOnly = false });

        var order = Db.GetOrder(id);
        if (order == null) return NotFound();

        // Missing: verify order.Owner == current user. That omission is the IDOR.
        return Content(
            $"Invoice #{order.Id} | Owner: {order.Owner} | {order.CustomerName} | Total: {order.Total}");
    }
}
