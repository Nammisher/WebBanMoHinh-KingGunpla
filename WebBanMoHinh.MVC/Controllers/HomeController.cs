using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebBanMoHinh.MVC.Models;

namespace WebBanMoHinh.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        // Tiêm HttpClient và Configuration để đọc file appsettings.json đã sửa dấu phẩy lúc nãy
        public HomeController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        public async Task<IActionResult> Index()
        {
            List<SanPhamViewModel> dsSanPham = new List<SanPhamViewModel>();

            try
            {
                // 1. Thực hiện gọi sang API Backend lấy danh sách sản phẩm
                var response = await _httpClient.GetAsync(_apiUrl + "SanPhams");

                if (response.IsSuccessStatusCode)
                {
                    // 2. Đọc chuỗi JSON từ API trả về
                    var jsonData = await response.Content.ReadAsStringAsync();

                    // 3. Giải mã dữ liệu JSON chuyển thành List đối tượng C#
                    dsSanPham = JsonSerializer.Deserialize<List<SanPhamViewModel>>(jsonData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Bỏ qua phân biệt hoa thường của JSON
                    }) ?? new List<SanPhamViewModel>();
                }
            }
            catch (Exception ex)
            {
                // Nếu API chưa bật hoặc lỗi, ghi tạm lỗi ra màn hình Console
                Console.WriteLine($"Lỗi kết nối API: {ex.Message}");
            }

            // 4. Truyền toàn bộ danh sách mô hình sang giao diện hiển thị
            return View(dsSanPham);
        }
        // GET: /Home/HuongDanRunner
        public IActionResult HuongDanRunner()
        {
            return View();
        }
        // GET: /Home/ChinhSachBaoHanh
        public IActionResult ChinhSachBaoHanh()
        {
            return View();
        }

        // GET: /Home/DieuKhoanDauGia
        public IActionResult DieuKhoanDauGia()
        {
            return View();
        }
    }
}