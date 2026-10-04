using System.Text.Json;
using VulnShop.Models;

namespace VulnShop.Services;

// Helpers for reading the current user and the session-backed cart.
public static class CartExtensions
{
    private const string CartKey = "cart";

    // The logged-in username, or null. The session cookie 'sid' holds the
    // username and is only ever set server-side to a known value.
    public static string? CurrentUser(this HttpContext ctx)
        => ctx.Request.Cookies["sid"];

    public static List<CartItem> GetCart(this HttpContext ctx)
    {
        var json = ctx.Session.GetString(CartKey);
        if (string.IsNullOrEmpty(json)) return new List<CartItem>();
        try { return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new(); }
        catch { return new List<CartItem>(); }
    }

    public static void SaveCart(this HttpContext ctx, List<CartItem> cart)
        => ctx.Session.SetString(CartKey, JsonSerializer.Serialize(cart));

    public static void ClearCart(this HttpContext ctx)
        => ctx.Session.Remove(CartKey);
}
