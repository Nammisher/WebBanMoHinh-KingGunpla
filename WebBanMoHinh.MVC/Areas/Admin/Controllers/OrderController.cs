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

        // 1. LẤY DANH SÁCH ĐỔ RA BẢNG (Đã sửa lại gọi đúng API AdminOrders)
        public async Task<IActionResult> Index()
        {
            var danhSach = new List<OrderHistoryViewModel>();
            var response = await _httpClient.GetAsync($"{_apiUrl}AdminOrders");
            
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

        // 2. HÀM TRUNG GIAN LẤY CHI TIẾT BILL (MỚI THÊM)
        [HttpGet]
        public async Task<IActionResult> GetDetail(int id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}AdminOrders/Detail/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                return Content(jsonData, "application/json"); // Trả thẳng JSON về cho Javascript
            }
            
            return NotFound(new { message = "Lỗi: Không tìm thấy chi tiết đơn hàng từ API!" });
        }

        // 3. CẬP NHẬT TRẠNG THÁI
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int maDonHang, string trangThaiMoi)
        {
            // Tạo đối tượng khớp với cấu trúc UpdateOrderDto bên API
            var payload = new { TrangThai = trangThaiMoi, MaVanDon = (string?)null };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            // Đã sửa lại đường dẫn gọi API UpdateStatus
            var response = await _httpClient.PutAsync($"{_apiUrl}AdminOrders/UpdateStatus/{maDonHang}", jsonContent);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });
            
            return Json(new { success = false, message = "Lỗi khi cập nhật trên máy chủ chỉ huy!" });
        }
    }
}