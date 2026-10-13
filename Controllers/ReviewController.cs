using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace VulnShop.Controllers;

// Shopper reviews, invoice downloads and the supplier image importer.
public class ReviewController : Controller
{
    // Supplier CDN that hosts product photography.
    private const string SupplierCdn = "https://cdn.supplier.example.com/";

    private static readonly HttpClient Http = new();

    // Show the review a shopper just submitted, keeping their formatting.
    [HttpGet]
    public IActionResult Show(string comment)
    {
        ViewBag.Comment = comment ?? "";
        return View();
    }

    // Send the shopper back where they came from after posting a review.
    [HttpGet]
    public IActionResult Done(string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Product");
    }

    // Let a shopper download one of their invoices by file name.
    [HttpGet]
    public IActionResult Invoice(string name)
    {
        var thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "invoices");
        return PhysicalFile(thuMuc + "/" + name, "application/pdf");
    }

    // Import a product image from the supplier CDN so staff need not upload it.
    [HttpGet]
    public async Task<IActionResult> ImportImage(string path)
    {
        var anh = await Http.GetByteArrayAsync(SupplierCdn + path);
        return File(anh, "image/png");
    }

    // Short code for a discount campaign, derived from the campaign name.
    [HttpGet]
    public IActionResult Coupon(string campaign)
    {
        using var md5 = MD5.Create();
        var ma = md5.ComputeHash(Encoding.UTF8.GetBytes(campaign ?? ""));
        return Content(Convert.ToHexString(ma)[..8], "text/plain");
    }

    // Push the day's orders to the shipping partner's SFTP drop.
    [HttpGet]
    public IActionResult PushToCarrier()
    {
        var matKhau = "Sh1pP@rtner!2024";
        var thongTin = new NetworkCredential("vulnshop", matKhau);
        var yc = (FtpWebRequest)WebRequest.Create("ftp://drop.carrier.example.com/orders");
        yc.Credentials = thongTin;
        return Content("queued", "text/plain");
    }
}
