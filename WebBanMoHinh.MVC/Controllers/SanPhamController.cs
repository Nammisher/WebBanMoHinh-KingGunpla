using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebBanMoHinh.MVC.Models;
using System.Text;
namespace WebBanMoHinh.MVC.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public SanPhamController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // 1. GET /SanPham/Index (Trang hiển thị tất cả sản phẩm + Gọi API Lọc)
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? maNsx, int? maDong, string? priceRange, string? sortBy)
        {
            // BƯỚC 1: Xử lý logic khoảng giá từ chuỗi (Ví dụ: "500-1000")
            decimal? minPrice = null;
            decimal? maxPrice = null;

            if (!string.IsNullOrEmpty(priceRange))
            {
                if (priceRange == "under500") maxPrice = 500000;
                else if (priceRange == "500-1000") { minPrice = 500000; maxPrice = 1000000; }
                else if (priceRange == "1000-2000") { minPrice = 1000000; maxPrice = 2000000; }
                else if (priceRange == "over2000") minPrice = 2000000;
            }

            // BƯỚC 2: Lưu lại trạng thái của bộ lọc vào ViewBag để hiển thị trên View giữ trạng thái nút chọn
            ViewBag.Search = search;
            ViewBag.MaNsx = maNsx;
            ViewBag.MaDong = maDong; // <--- ĐÃ THÊM: Lưu trạng thái Dòng sản phẩm
            ViewBag.PriceRange = priceRange;
            ViewBag.SortBy = string.IsNullOrEmpty(sortBy) ? "newest" : sortBy; // Mặc định là newest

            // BƯỚC 3: Xây dựng chuỗi Query String gửi sang API
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={search}");
            if (maNsx.HasValue) queryParams.Add($"maNsx={maNsx}");
            if (maDong.HasValue) queryParams.Add($"maDong={maDong}"); // <--- ĐÃ THÊM: Gửi mã Dòng sang API
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={sortBy}");

            string queryString = string.Join("&", queryParams);
            string requestUrl = $"{_apiUrl}SanPhams/LocSanPham?{queryString}";

            List<SanPhamViewModel> danhSach = new List<SanPhamViewModel>();

            // BƯỚC 4: Gọi API và hứng dữ liệu JSON
            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    danhSach = JsonSerializer.Deserialize<List<SanPhamViewModel>>(jsonData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<SanPhamViewModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gọi API Lọc Sản Phẩm: {ex.Message}");
            }

            // Đẩy dữ liệu ra ngoài file View Index.cshtml
            return View(danhSach);
        }

        // GET: /SanPham/ChiTiet/1
        public async Task<IActionResult> ChiTiet(int id)
        {
            SanPhamViewModel? sanPham = null;

            try
            {
                // Gọi sang API lấy chi tiết 1 sản phẩm theo ID
                var response = await _httpClient.GetAsync(_apiUrl + $"SanPhams/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    sanPham = JsonSerializer.Deserialize<SanPhamViewModel>(jsonData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gọi API chi tiết sản phẩm: {ex.Message}");
            }

            if (sanPham == null)
            {
                return NotFound(); // Trả về trang 404 nếu không tìm thấy mô hình
            }

            return View(sanPham);
        }
        // POST: /SanPham/GuiDanhGia
        [HttpPost]
        public async Task<IActionResult> GuiDanhGia(int MaSp, int SoSao, string NoiDung)
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) return Json(new { success = false, message = "Phiên đăng nhập hết hạn!" });

            var danhGia = new BinhLuanDanhGiaViewModel
            {
                MaSp = MaSp,
                TenDangNhap = username,
                SoSao = SoSao,
                NoiDung = NoiDung,
                NgayBinhLuan = DateTime.Now
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(danhGia), Encoding.UTF8, "application/json");
            
            // ĐÃ SỬA DÒNG NÀY: Trỏ đúng vào Controller BinhLuanDanhGias của Web API
            var response = await _httpClient.PostAsync($"{_apiUrl}BinhLuanDanhGias", jsonContent);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true, message = "Cảm ơn Pilot đã đánh giá!" });
            else
                return Json(new { success = false, message = "Lỗi khi lưu đánh giá!" });
        }
    }
}