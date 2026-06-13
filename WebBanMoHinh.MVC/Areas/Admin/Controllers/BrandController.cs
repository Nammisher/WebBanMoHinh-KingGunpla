using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;
using WebBanMoHinh.MVC.Models;
//Danh mục nhà sản xuất
namespace WebBanMoHinh.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public BrandController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        public async Task<IActionResult> Index()
        {
            var danhSach = new List<NhaSanXuatViewModel>();
            var response = await _httpClient.GetAsync($"{_apiUrl}NhaSanXuats");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                danhSach = JsonSerializer.Deserialize<List<NhaSanXuatViewModel>>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NhaSanXuatViewModel>();
            }
            return View(danhSach);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] NhaSanXuatViewModel model)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response;

            if (model.MaNsx == 0) response = await _httpClient.PostAsync($"{_apiUrl}NhaSanXuats", jsonContent);
            else response = await _httpClient.PutAsync($"{_apiUrl}NhaSanXuats/{model.MaNsx}", jsonContent);

            if (response.IsSuccessStatusCode) return Json(new { success = true });
            return Json(new { success = false, message = "Lỗi kết nối hoặc dữ liệu không hợp lệ!" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}NhaSanXuats/{id}");
            if (response.IsSuccessStatusCode) return Json(new { success = true });
            
            var errorResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            string msg = errorResult != null && errorResult.ContainsKey("message") ? errorResult["message"] : "Không thể xóa danh mục này!";
            return Json(new { success = false, message = msg });
        }
    }
}