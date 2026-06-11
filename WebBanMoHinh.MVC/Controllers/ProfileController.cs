using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using WebBanMoHinh.MVC.Models;

namespace WebBanMoHinh.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public ProfileController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

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

            TempData["Error"] = "Không thể kết nối tới máy chủ!";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Profile/UpdateInfo (ĐÃ SỬA TÊN VÀ ĐƯỜNG DẪN)
        [HttpPost]
        public async Task<IActionResult> UpdateInfo(UserProfileViewModel model)
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) return Json(new { success = false, message = "Phiên làm việc hết hạn!" });

            model.TenDangNhap = username;

            // XỬ LÝ UPLOAD AVATAR
            if (model.AvatarUpload != null && model.AvatarUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarUpload.CopyToAsync(fileStream);
                }
                model.Avatar = "avatars/" + uniqueFileName; 
            }

            // GỌI API CẬP NHẬT
            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_apiUrl}TaiKhoans/CapNhatProfile", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(model.Avatar))
                {
                    HttpContext.Session.SetString("UserAvatar", model.Avatar);
                }
                // TRẢ VỀ JSON THAY VÌ REDIRECT
                return Json(new { success = true, message = "Cập nhật hồ sơ thành công!" });
            }
            else
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật thông tin!" });
            }
        }
    }
}