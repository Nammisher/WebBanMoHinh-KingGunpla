using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
    }
}