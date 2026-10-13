using Microsoft.AspNetCore.Mvc;
using VulnShop.Data;

namespace VulnShop.Controllers;

public class ProductController : Controller
{
    // Catalog with optional category filter, name search and price range.
    // Every input flows into a parameterized query in Db.GetProducts.
    public IActionResult Index(string? category, string? q, decimal? minPrice, decimal? maxPrice)
    {
        ViewBag.Categories = Db.GetCategories();
        ViewBag.Category = category;
        ViewBag.Query = q;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;
        var products = Db.GetProducts(category, q, minPrice, maxPrice);
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
