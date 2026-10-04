using Microsoft.Data.Sqlite;
using VulnShop.Models;

namespace VulnShop.Data;

// Thin data-access layer over a local SQLite database. All queries are
// parameterized; the database is recreated with sample data on startup.
public static class Db
{
    public const string ConnectionString = "Data Source=vulnshop.db";

    public static void Init()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            DROP TABLE IF EXISTS Products;
            CREATE TABLE Products (
                Id          INTEGER PRIMARY KEY,
                Name        TEXT NOT NULL,
                Category    TEXT NOT NULL,
                Price       REAL NOT NULL,
                Description TEXT NOT NULL,
                Icon        TEXT NOT NULL,
                Color       TEXT NOT NULL
            );
            INSERT INTO Products (Id, Name, Category, Price, Description, Icon, Color) VALUES
                (1,  'TechPro Mechanical Keyboard', 'Keyboards',   1200000, 'Hot-swappable mechanical keyboard with RGB backlight and PBT keycaps.', '⌨️', '#6366f1'),
                (2,  'Silent Wireless Mouse',       'Mice',         450000, 'Ergonomic 2.4GHz wireless mouse with silent clicks and 6 buttons.',     '🖱️', '#0ea5e9'),
                (3,  '27-inch QHD Monitor',         'Monitors',    5600000, '27-inch 1440p IPS display, 165Hz, with a height-adjustable stand.',     '🖥️', '#14b8a6'),
                (4,  'Noise-Cancelling Headphones', 'Audio',       2300000, 'Over-ear ANC headphones with 40h battery life and USB-C fast charge.',  '🎧', '#f59e0b'),
                (5,  'Full-HD Webcam',              'Accessories',  890000, '1080p60 webcam with auto-focus, dual mics and a privacy shutter.',      '📷', '#ef4444'),
                (6,  'Compact 65% Keyboard',        'Keyboards',    980000, 'Space-saving 65% layout, gasket mount, wired USB-C.',                   '⌨️', '#8b5cf6'),
                (7,  'Gaming Mouse 8K',             'Mice',         790000, '8000Hz polling gaming mouse, 26K DPI sensor, lightweight shell.',       '🖱️', '#3b82f6'),
                (8,  'Ultrawide 34-inch Monitor',   'Monitors',    8900000, '34-inch UWQHD curved monitor, 144Hz, USB-C 90W power delivery.',        '🖥️', '#10b981'),
                (9,  'Studio USB Microphone',       'Audio',       1650000, 'Cardioid USB condenser microphone with zero-latency monitoring.',      '🎙️', '#f97316'),
                (10, 'Laptop Stand Aluminium',      'Accessories',  520000, 'Adjustable aluminium laptop stand, foldable and travel friendly.',      '💻', '#e11d48'),
                (11, 'Mechanical Numpad',           'Keyboards',    390000, 'Standalone wireless mechanical numpad for finance and editing work.',   '🔢', '#7c3aed'),
                (12, 'USB-C Hub 8-in-1',            'Accessories',  650000, '8-in-1 USB-C hub: HDMI 4K, 100W PD, SD, and three USB-A ports.',        '🔌', '#dc2626');

            DROP TABLE IF EXISTS Users;
            CREATE TABLE Users (Id INTEGER PRIMARY KEY, Username TEXT, DisplayName TEXT);
            INSERT INTO Users (Id, Username, DisplayName) VALUES
                (1, 'alice', 'Alice Nguyen'),
                (2, 'bob',   'Bob Tran');

            DROP TABLE IF EXISTS Orders;
            CREATE TABLE Orders (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                Owner        TEXT NOT NULL,
                CustomerName TEXT NOT NULL,
                Address      TEXT NOT NULL,
                Total        REAL NOT NULL,
                CreatedAt    TEXT NOT NULL
            );
            DROP TABLE IF EXISTS OrderLines;
            CREATE TABLE OrderLines (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId     INTEGER NOT NULL,
                ProductName TEXT NOT NULL,
                Price       REAL NOT NULL,
                Quantity    INTEGER NOT NULL
            );

            -- Two sample orders so the 'My orders' page is not empty.
            INSERT INTO Orders (Id, Owner, CustomerName, Address, Total, CreatedAt) VALUES
                (1, 'alice', 'Alice Nguyen', '12 Hai Ba Trung, District 1, HCMC', 1650000, '2026-09-30 09:14'),
                (2, 'bob',   'Bob Tran',     '48 Le Loi, District 1, HCMC',       2300000, '2026-10-01 16:40');
            INSERT INTO OrderLines (OrderId, ProductName, Price, Quantity) VALUES
                (1, 'TechPro Mechanical Keyboard', 1200000, 1),
                (1, 'Silent Wireless Mouse',        450000, 1),
                (2, 'Noise-Cancelling Headphones', 2300000, 1);
        ";
        cmd.ExecuteNonQuery();
    }

    public static List<Product> GetProducts(string? category = null, string? search = null)
    {
        var list = new List<Product>();
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        var sql = "SELECT Id, Name, Category, Price, Description, Icon, Color FROM Products WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(category))
        {
            sql += " AND Category = @category";
            cmd.Parameters.AddWithValue("@category", category);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " AND Name LIKE @search";
            cmd.Parameters.AddWithValue("@search", "%" + search + "%");
        }
        sql += " ORDER BY Id";
        cmd.CommandText = sql;
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Read(r));
        return list;
    }

    public static Product? GetProduct(int id)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Category, Price, Description, Icon, Color FROM Products WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Read(r) : null;
    }

    public static List<string> GetCategories()
    {
        var list = new List<string>();
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT Category FROM Products ORDER BY Category";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(r.GetString(0));
        return list;
    }

    public static int CreateOrder(Order order)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Orders (Owner, CustomerName, Address, Total, CreatedAt)
                            VALUES (@o, @c, @a, @t, @d);
                            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@o", order.Owner);
        cmd.Parameters.AddWithValue("@c", order.CustomerName);
        cmd.Parameters.AddWithValue("@a", order.Address);
        cmd.Parameters.AddWithValue("@t", order.Total);
        cmd.Parameters.AddWithValue("@d", order.CreatedAt);
        var orderId = Convert.ToInt32(cmd.ExecuteScalar());

        foreach (var line in order.Lines)
        {
            var lc = conn.CreateCommand();
            lc.CommandText = @"INSERT INTO OrderLines (OrderId, ProductName, Price, Quantity)
                               VALUES (@oid, @n, @p, @q)";
            lc.Parameters.AddWithValue("@oid", orderId);
            lc.Parameters.AddWithValue("@n", line.ProductName);
            lc.Parameters.AddWithValue("@p", line.Price);
            lc.Parameters.AddWithValue("@q", line.Quantity);
            lc.ExecuteNonQuery();
        }
        return orderId;
    }

    public static List<Order> GetOrdersByOwner(string owner)
    {
        var list = new List<Order>();
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Owner, CustomerName, Address, Total, CreatedAt FROM Orders WHERE Owner = @o ORDER BY Id DESC";
        cmd.Parameters.AddWithValue("@o", owner);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(ReadOrder(r));
        return list;
    }

    public static Order? GetOrder(int id)
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Owner, CustomerName, Address, Total, CreatedAt FROM Orders WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        Order? order = null;
        using (var r = cmd.ExecuteReader())
            if (r.Read()) order = ReadOrder(r);
        if (order == null) return null;

        var lc = conn.CreateCommand();
        lc.CommandText = "SELECT ProductName, Price, Quantity FROM OrderLines WHERE OrderId = @id";
        lc.Parameters.AddWithValue("@id", id);
        using var lr = lc.ExecuteReader();
        while (lr.Read())
            order.Lines.Add(new OrderLine
            {
                ProductName = lr.GetString(0),
                Price = (decimal)lr.GetDouble(1),
                Quantity = lr.GetInt32(2),
            });
        return order;
    }

    private static Product Read(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Category = r.GetString(2),
        Price = (decimal)r.GetDouble(3),
        Description = r.GetString(4),
        Icon = r.GetString(5),
        Color = r.GetString(6),
    };

    private static Order ReadOrder(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Owner = r.GetString(1),
        CustomerName = r.GetString(2),
        Address = r.GetString(3),
        Total = (decimal)r.GetDouble(4),
        CreatedAt = r.GetString(5),
    };
}
