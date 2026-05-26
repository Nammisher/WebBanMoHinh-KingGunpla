using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using WebBanMoHinh.MVC.Models;
using System.IO; // Thêm thư viện này để xử lý File và Thư mục

namespace WebBanMoHinh.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public ProfileController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Lấy URL gốc "http://localhost:5298/api/" giống như bên OrderController
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            // ĐÃ SỬA: Thêm chính xác phân cấp "TaiKhoans/" vào trước endpoint
            var response = await _httpClient.GetAsync($"{_apiUrl}TaiKhoans/Profile/{username}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var profileData = JsonSerializer.Deserialize<UserProfileViewModel>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return View(profileData);
            }

            TempData["Error"] = "Không thể kết nối tới máy chủ để tải hồ sơ Pilot!";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Profile
        [HttpPost]
        public async Task<IActionResult> Index(UserProfileViewModel model)
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            model.TenDangNhap = username;

            if (!string.IsNullOrEmpty(model.MatKhauMoi) && model.MatKhauMoi != model.NhapLaiMatKhauMoi)
            {
                TempData["Error"] = "Mật khẩu mới và xác nhận mật khẩu không khớp nhau!";
                return View(model);
            }

            // =========================================================
            // BƯỚC 4: XỬ LÝ UPLOAD AVATAR NẾU CÓ FILE ẢNH ĐƯỢC CHỌN
            // =========================================================
            if (model.AvatarUpload != null && model.AvatarUpload.Length > 0)
            {
                // Tạo thư mục wwwroot/images/avatars nếu chưa có
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Tạo tên file ngẫu nhiên để tránh trùng lặp (Ví dụ: 9a2b3c..._avatar.png)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Copy ảnh vật lý vào thư mục MVC
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarUpload.CopyToAsync(fileStream);
                }

                // Gán tên file mới vào Model để chuẩn bị gửi sang API lưu vào Database
                model.Avatar = "avatars/" + uniqueFileName; 
            }
            // =========================================================

            // ĐÃ SỬA: Thêm chính xác "TaiKhoans/" vào lệnh gọi PUT cập nhật dữ liệu
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiUrl}TaiKhoans/CapNhatProfile", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(model.Avatar))
                {
                    HttpContext.Session.SetString("UserAvatar", model.Avatar);
                }
                TempData["Success"] = "Cập nhật hồ sơ Pilot thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                var errorData = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(errorData);
                string message = doc.RootElement.TryGetProperty("message", out var msgElement) 
                    ? msgElement.GetString() ?? "Lỗi cập nhật!" 
                    : "Lỗi hệ thống!";
                
                TempData["Error"] = message;
            }

            return View(model);
        }
    }
}