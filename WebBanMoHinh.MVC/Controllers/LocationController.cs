using Microsoft.AspNetCore.Mvc;

namespace WebBanMoHinh.MVC.Controllers
{
    public class LocationController : Controller
    {
        // Dùng static HttpClient để KHÔNG BỊ LỖI thiếu đăng ký trong Program.cs
        private static readonly HttpClient _httpClient = new HttpClient();

        [HttpGet]
        [Route("Location/GetProvinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var response = await _httpClient.GetAsync("https://esgoo.net/api-tinhthanh/1/0.htm");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet]
        [Route("Location/GetDistricts/{id}")]
        public async Task<IActionResult> GetDistricts(string id)
        {
            var response = await _httpClient.GetAsync($"https://esgoo.net/api-tinhthanh/2/{id}.htm");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet]
        [Route("Location/GetWards/{id}")]
        public async Task<IActionResult> GetWards(string id)
        {
            var response = await _httpClient.GetAsync($"https://esgoo.net/api-tinhthanh/3/{id}.htm");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}