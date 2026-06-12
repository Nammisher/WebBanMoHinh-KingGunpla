using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Thêm thư viện này

namespace WebBanMoHinh.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // <--- KHÓA CỬA: CHỈ ADMIN MỚI ĐƯỢC VÀO ĐÂY
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}