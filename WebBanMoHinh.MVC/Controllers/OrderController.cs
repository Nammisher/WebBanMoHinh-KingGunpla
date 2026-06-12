using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text; // Thêm thư viện này để dùng Encoding.UTF8 cho JSON
using WebBanMoHinh.MVC.Models;

namespace WebBanMoHinh.MVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public OrderController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // GET: /Order/History
        public async Task<IActionResult> History()
        {
            // 1. Kiểm tra xem khách đã đăng nhập chưa
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            var lichSuDonHang = new List<OrderHistoryViewModel>();

            // 2. Gọi API lấy lịch sử
            var response = await _httpClient.GetAsync($"{_apiUrl}DonHangs/LichSu/{username}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                lichSuDonHang = JsonSerializer.Deserialize<List<OrderHistoryViewModel>>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<OrderHistoryViewModel>();
            }

            // 3. Trả về View hiển thị
            return View(lichSuDonHang);
        }

        // TÍNH NĂNG MỚI: KHÁCH HÀNG XÁC NHẬN ĐÃ NHẬN ĐƯỢC HÀNG
        // POST: /Order/DaNhanHang
        [HttpPost]
        public async Task<IActionResult> DaNhanHang(int maDonHang)
        {
            // 1. Kiểm tra session
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) 
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn!" });
            }

            // 2. Đóng gói trạng thái mới ("Hoàn thành") thành định dạng JSON string
            var jsonContent = new StringContent(JsonSerializer.Serialize("Hoàn thành"), Encoding.UTF8, "application/json");
            
            // 3. Gọi Web API "DoiTrangThai" để cập nhật Database
            var response = await _httpClient.PutAsync($"{_apiUrl}DonHangs/DoiTrangThai/{maDonHang}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Xác nhận nhận hàng thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái đơn hàng trên hệ thống." });
            }
        }
    }
}