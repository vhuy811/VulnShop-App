using Microsoft.AspNetCore.Mvc;
using VulnShop.Data;
using VulnShop.Models;
using VulnShop.Services;

namespace VulnShop.Controllers;

public class OrderController : Controller
{
    // Checkout form. Requires a non-empty cart and a logged-in user.
    [HttpGet]
    public IActionResult Checkout()
    {
        if (HttpContext.CurrentUser() is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Order/Checkout" });
        var cart = HttpContext.GetCart();
        if (cart.Count == 0) return RedirectToAction("Index", "Cart");
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PlaceOrder(string customerName, string address)
    {
        var user = HttpContext.CurrentUser();
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Order/Checkout" });

        var cart = HttpContext.GetCart();
        if (cart.Count == 0) return RedirectToAction("Index", "Cart");

        var order = new Order
        {
            Owner = user,
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? user : customerName,
            Address = address ?? "",
            Total = cart.Sum(c => c.LineTotal),
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
            Lines = cart.Select(c => new OrderLine { ProductName = c.Name, Price = c.Price, Quantity = c.Quantity }).ToList(),
        };
        var id = Db.CreateOrder(order);
        HttpContext.ClearCart();
        return RedirectToAction(nameof(Details), new { id });
    }

    // Order history for the logged-in user.
    [HttpGet]
    public IActionResult MyOrders()
    {
        var user = HttpContext.CurrentUser();
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Order/MyOrders" });
        return View(Db.GetOrdersByOwner(user));
    }

    // Order detail. This is an object-level resource: the ownership check
    // below is what prevents IDOR/BOLA (CWE-639). The pipeline verifies it
    // at runtime via .devsecops/idor.json -> DAST-idor.
    [HttpGet]
    public IActionResult Details(int id)
    {
        var user = HttpContext.CurrentUser();
        if (user is null)
            return RedirectToAction("Login", "Account", new { returnUrl = "/Order/Details?id=" + id });

        var order = Db.GetOrder(id);
        if (order == null) return NotFound();

        // Object-level authorization: only the owner may view the order.
        if (!string.Equals(order.Owner, user, StringComparison.Ordinal))
            return StatusCode(403, "You are not allowed to view this order.");

        return View(order);
    }
}
