using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;
using WebBanMoHinh.MVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebBanMoHinh.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly IWebHostEnvironment _env;

        public ProductController(HttpClient httpClient, IConfiguration configuration, IWebHostEnvironment env)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
            _env = env; // Dùng để truy cập thư mục lưu file ảnh
        }

        // 1. TẢI GIAO DIỆN VÀ ĐỔ DỮ LIỆU
        public async Task<IActionResult> Index()
        {
            // Tải 3 Danh mục lõi từ API để làm Dropdown trong Modal
            ViewBag.LoaiSP = new SelectList(await FetchData<LoaiSanPhamViewModel>("LoaiSanPhams"), "MaLoai", "TenLoai");
            ViewBag.DongSP = new SelectList(await FetchData<DongSanPhamViewModel>("DongSanPhams"), "MaDong", "TenDong");
            ViewBag.NhaSX = new SelectList(await FetchData<NhaSanXuatViewModel>("NhaSanXuats"), "MaNsx", "TenNsx");

            // Tải danh sách toàn bộ Sản phẩm
            var products = await FetchData<AdminProductDto>("AdminSanPhams");
            return View(products);
        }

        // Hàm hỗ trợ tái sử dụng code gọi API lấy danh sách
        private async Task<List<T>> FetchData<T>(string endpoint)
        {
            var res = await _httpClient.GetAsync($"{_apiUrl}{endpoint}");
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>();
            }
            return new List<T>();
        }

        // 2. LƯU SẢN PHẨM & XỬ LÝ UPLOAD ẢNH
        [HttpPost]
        public async Task<IActionResult> SaveData([FromForm] AdminProductDto model, IFormFile? ImageFile)
        {
            try
            {
                // Nếu Admin có chọn tải ảnh mới lên
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string ext = Path.GetExtension(ImageFile.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + ext; // Tạo tên ảnh không trùng
                    string folder = Path.Combine(_env.WebRootPath, "images", "ImageProduct");
                    
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    
                    using (var stream = new FileStream(Path.Combine(folder, uniqueFileName), FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    model.HinhAnhChinh = uniqueFileName; // Gắn tên ảnh vào Model
                }

                // Đẩy dữ liệu sang Tổng kho API
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                HttpResponseMessage res;

                if (model.MaSp == 0) res = await _httpClient.PostAsync($"{_apiUrl}AdminSanPhams", jsonContent); // Thêm mới
                else res = await _httpClient.PutAsync($"{_apiUrl}AdminSanPhams/{model.MaSp}", jsonContent); // Sửa

                if (res.IsSuccessStatusCode) return Json(new { success = true });
                return Json(new { success = false, message = "Lỗi khi lưu dữ liệu vào API!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // 3. XÓA MỀM SẢN PHẨM
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _httpClient.DeleteAsync($"{_apiUrl}AdminSanPhams/{id}");
            if (res.IsSuccessStatusCode) return Json(new { success = true });
            return Json(new { success = false, message = "Không thể xóa sản phẩm này!" });
        }
    }

    // Class phụ dùng để hứng dữ liệu chuẩn xác từ API trả về
    public class AdminProductDto
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; } = null!;
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
        public string? HinhAnhChinh { get; set; }
        public bool IsHot { get; set; }
        public int? MaLoai { get; set; }
        public string? TenLoai { get; set; }
        public int? MaDong { get; set; }
        public string? TenDong { get; set; }
        public int? MaNsx { get; set; }
        public string? TenNsx { get; set; }
        public string? TiLe { get; set; }
        public string? KichThuoc { get; set; }
        public string? ChatLieu { get; set; }
        public string? MoTa { get; set; }
    }
}