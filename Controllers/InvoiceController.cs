using Microsoft.AspNetCore.Mvc;
using VulnShop.Data;
using VulnShop.Services;

namespace VulnShop.Controllers;

// === NHANH TRINH DIEN - GIEO LOI CO Y, KHONG MERGE ===
//
// Trang tra cuu hoa don. Ma nguon duoi day KHONG co loi nao ma phan tich tinh
// nhan ra duoc: khong noi chuoi SQL, khong dung dau vao de dung HTML, khong
// bam yeu, khong chuyen huong theo tham so, khong doc tep theo ten nguoi dung.
// Truy van da tham so hoa san trong Db.GetOrder.
//
// Lo hong o day la CWE-639 (IDOR/BOLA): thieu MOT cau kiem tra xem hoa don co
// phai cua nguoi dang dang nhap khong. Khong cong cu tinh nao bat duoc, vi
// "don hang nay cua ai" la mot rang buoc NGHIEP VU, khong phai mot mau ma.
// Phai dang nhap bang hai tai khoan roi doc cheo moi thay - dung viec cua lop
// DAST-idor.
public class InvoiceController : Controller
{
    // Dat ten Detail chu khong phai View, de khong trung ten voi phuong thuc
    // View() ma Controller ke thua san.
    public IActionResult Detail(int id)
    {
        var user = HttpContext.CurrentUser();
        if (user is null) return Redirect("/Account/Login");

        var order = Db.GetOrder(id);
        if (order == null) return NotFound();

        // THIEU O DAY: if (order.Owner != user) return Forbid();

        return Content(
            $"Hoa don #{order.Id}\nKhach hang: {order.CustomerName}\n" +
            $"Dia chi: {order.Address}\nTong: {order.Total:n0} d\nNgay: {order.CreatedAt}",
            "text/plain");
    }
}
