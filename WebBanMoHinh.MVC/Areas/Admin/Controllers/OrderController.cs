using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using WebBanMoHinh.MVC.Models;
using System.Text;

namespace WebBanMoHinh.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Khóa bảo mật
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public OrderController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // Lấy danh sách đổ ra bảng
        public async Task<IActionResult> Index()
        {
            var danhSach = new List<OrderHistoryViewModel>();
            var response = await _httpClient.GetAsync($"{_apiUrl}DonHangs/All");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                danhSach = JsonSerializer.Deserialize<List<OrderHistoryViewModel>>(jsonData, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<OrderHistoryViewModel>();
            }
            return View(danhSach);
        }

        // Admin bấm nút đổi trạng thái (Gọi hàm PUT của API đã có sẵn)
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int maDonHang, string trangThaiMoi)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(trangThaiMoi), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiUrl}DonHangs/DoiTrangThai/{maDonHang}", jsonContent);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });
            
            return Json(new { success = false, message = "Lỗi khi cập nhật trên máy chủ chỉ huy!" });
        }
    }
}