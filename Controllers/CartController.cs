using Microsoft.AspNetCore.Mvc;
using VulnShop.Data;
using VulnShop.Models;
using VulnShop.Services;

namespace VulnShop.Controllers;

public class CartController : Controller
{
    public IActionResult Index()
    {
        return View(HttpContext.GetCart());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(int id, int quantity = 1)
    {
        var product = Db.GetProduct(id);
        if (product == null) return NotFound();
        if (quantity < 1) quantity = 1;

        var cart = HttpContext.GetCart();
        var line = cart.FirstOrDefault(c => c.ProductId == id);
        if (line == null)
            cart.Add(new CartItem { ProductId = id, Name = product.Name, Price = product.Price, Quantity = quantity });
        else
            line.Quantity += quantity;

        HttpContext.SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var cart = HttpContext.GetCart();
        cart.RemoveAll(c => c.ProductId == id);
        HttpContext.SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }
}
