using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;
using WebBanMoHinh.MVC.Models;
//Danh mục loại sản phẩm
namespace WebBanMoHinh.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public CategoryController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // 1. LẤY DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var danhSach = new List<LoaiSanPhamViewModel>();
            var response = await _httpClient.GetAsync($"{_apiUrl}LoaiSanPhams");
            
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                danhSach = JsonSerializer.Deserialize<List<LoaiSanPhamViewModel>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<LoaiSanPhamViewModel>();
            }
            return View(danhSach);
        }

        // 2. THÊM MỚI VÀ CẬP NHẬT (Gộp chung để tối ưu code)
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] LoaiSanPhamViewModel model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response;

            if (model.MaLoai == 0) 
                response = await _httpClient.PostAsync($"{_apiUrl}LoaiSanPhams", jsonContent); // GỌI POST NẾU LÀ THÊM MỚI
            else 
                response = await _httpClient.PutAsync($"{_apiUrl}LoaiSanPhams/{model.MaLoai}", jsonContent); // GỌI PUT NẾU LÀ SỬA

            if (response.IsSuccessStatusCode) return Json(new { success = true });
            
            return Json(new { success = false, message = "Lỗi kết nối hoặc dữ liệu không hợp lệ!" });
        }

        // 3. XÓA DANH MỤC
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}LoaiSanPhams/{id}");
            if (response.IsSuccessStatusCode) return Json(new { success = true });
            
            // Hứng thông báo lỗi từ API (VD: Chặn xóa vì đang có sản phẩm)
            var errorResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            string msg = errorResult != null && errorResult.ContainsKey("message") ? errorResult["message"] : "Không thể xóa danh mục này!";
            
            return Json(new { success = false, message = msg });
        }
    }
}