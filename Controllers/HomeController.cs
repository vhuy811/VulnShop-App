using Microsoft.AspNetCore.Mvc;
using VulnShop.Data;

namespace VulnShop.Controllers;

public class HomeController : Controller
{
    // Landing page: a few featured products plus the category list.
    public IActionResult Index()
    {
        ViewBag.Categories = Db.GetCategories();
        var featured = Db.GetProducts();
        // Show the first 6 products as "featured".
        return View(featured.Take(6).ToList());
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
