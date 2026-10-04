namespace VulnShop.Models;

// Catalog item shown on the storefront.
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";   // emoji used as a lightweight product image
    public string Color { get; set; } = "";  // background accent for the product tile
}

// One line of the session cart.
public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}

// A placed order and its lines.
public class Order
{
    public int Id { get; set; }
    public string Owner { get; set; } = "";         // username that placed the order
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public decimal Total { get; set; }
    public string CreatedAt { get; set; } = "";
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => Price * Quantity;
}
