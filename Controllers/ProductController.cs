using Microsoft.AspNetCore.Mvc;
using VulnShop.Data;

namespace VulnShop.Controllers;

public class ProductController : Controller
{
    // Catalog with optional category filter and name search.
    // Both inputs flow into parameterized queries in Db.GetProducts.
    public IActionResult Index(string? category, string? q)
    {
        ViewBag.Categories = Db.GetCategories();
        ViewBag.Category = category;
        ViewBag.Query = q;
        var products = Db.GetProducts(category, q);
        return View(products);
    }

    // Product detail page.
    public IActionResult Details(int id)
    {
        var product = Db.GetProduct(id);
        if (product == null) return NotFound();
        return View(product);
    }
}
