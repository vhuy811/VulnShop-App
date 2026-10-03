using VulnShop.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ===== G3.3: Security header cho MOI phan hoi =====
// Lam sach tang runtime cua DAST: CSP, X-Frame-Options, X-Content-Type-Options
// (rule runtime trong dast_scan.py + chinh-sach-zap.json soi dung ba header nay).
// Dat o DAU pipeline de phu ca trang tinh (wwwroot), trang dong va trang loi.
// App co y chay HTTP thuan cho ZAP nen HSTS chi co tac dung khi trien khai HTTPS.
app.Use(async (context, next) =>
{
    var h = context.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "no-referrer";
    // style 'unsafe-inline' de Bootstrap chen style dong duoc; script buoc 'self'.
    h["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; font-src 'self'; object-src 'none'; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Co y KHONG dung app.UseHttpsRedirection() de OWASP ZAP quet duoc qua HTTP thuan.
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Tao lai CSDL SQLite voi du lieu mau moi lan khoi dong.
Db.Init();

app.Run();
