using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using WebBanMoHinh.MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
namespace WebBanMoHinh.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        // Đường dẫn gốc trỏ sang Controller TaiKhoans của API
        private readonly string _apiUrl = "http://localhost:5298/api/TaiKhoans/"; 

        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 1. GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        // 2. POST: /Account/Login (XỬ LÝ ĐĂNG NHẬP & PHỤC HỒI GIỎ HÀNG CŨ)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var requestBody = new { Username = model.TenDangNhap, Password = model.MatKhau };
                var response = await _httpClient.PostAsJsonAsync(_apiUrl + "DangNhap", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc phản hồi từ API để lấy thêm trường Avatar
                    var apiResult = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

                    // 1. Lưu Session Đăng nhập của Pilot trên MVC
                    HttpContext.Session.SetString("UserSession", model.TenDangNhap);

                    // ========================================================
                    // LƯU AVATAR VÀO SESSION KHI ĐĂNG NHẬP THƯỜNG
                    // ========================================================
                    if (apiResult != null && apiResult.ContainsKey("avatar") && apiResult["avatar"] != null)
                    {
                        HttpContext.Session.SetString("UserAvatar", apiResult["avatar"].ToString()!);
                    }

                    // 2. GỌI SANG API CART CONTROLLER ĐỂ LẤY LẠI GIỎ HÀNG CŨ CỦA USER NÀY
                    try
                    {
                        var cartResponse = await _httpClient.GetAsync($"http://localhost:5298/api/Cart/GetCart/{model.TenDangNhap}");
                        if (cartResponse.IsSuccessStatusCode)
                        {
                            var cartData = await cartResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                            if (cartData != null && cartData.ContainsKey("cartJson") && !string.IsNullOrEmpty(cartData["cartJson"]))
                            {
                                // Đổ chuỗi JSON giỏ hàng cũ vào lại Session "GioHang" của trình duyệt
                                HttpContext.Session.SetString("GioHang", cartData["cartJson"]);
                            }
                        }
                    }
                    catch 
                    { 
                        // Tạm thời bỏ qua nếu kết nối API Giỏ hàng gặp trục trặc để user vẫn login được
                    }

                    // Điều hướng sau khi login thành công
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    string errorMsg = errorResult != null && errorResult.ContainsKey("message") 
                        ? errorResult["message"] 
                        : "Tài khoản hoặc mật khẩu không chính xác!";

                    ModelState.AddModelError("", errorMsg);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi kết nối Server API: {ex.Message}");
            }

            return View(model);
        }

        // 3. GET: /Account/Logout (CẤT GIỎ HÀNG VÀO DB VÀ QUÉT SẠCH BỘ NHỚ TẠM)
        public async Task<IActionResult> Logout()
        {
            // Lấy tên Pilot và chuỗi JSON giỏ hàng hiện tại trước khi xóa sạch
            string? username = HttpContext.Session.GetString("UserSession");
            string? currentCartJson = HttpContext.Session.GetString("GioHang"); 

            // BẮN GIỎ HÀNG SANG API ĐỂ CẤT VÀO DATABASE SQL SERVER TRƯỚC
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(currentCartJson))
            {
                try
                {
                    var saveBody = new { Username = username, CartJson = currentCartJson };
                    // Gọi sang CartController bên API để lưu trữ vào cột DiaChi của bảng TaiKhoan
                    await _httpClient.PostAsJsonAsync("http://localhost:5298/api/Cart/SaveCart", saveBody);
                }
                catch 
                {
                    // Phòng trường hợp API mất kết nối thì luồng Logout vẫn chạy tiếp, không bị treo trang
                }
            }

            // LỆNH THẦN THÁNH - QUÉT SẠCH MỌI TRẠNG THÁI TRÊN TRÌNH DUYỆT
            // Giỏ hàng, tài khoản và AVATAR lập tức bốc hơi trên giao diện (trống trơn 100%)
            HttpContext.Session.Clear(); 

            return RedirectToAction("Index", "Home");
        }

        // 4. GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();  
        }

        // 5. POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var requestBody = new
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = model.MatKhau,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai 
                };

                var response = await _httpClient.PostAsJsonAsync(_apiUrl + "DangKy", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Kích hoạt tài khoản Pilot thành công! Mời bạn đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    string errorMsg = errorResult != null && errorResult.ContainsKey("message") 
                        ? errorResult["message"] 
                        : "Đăng ký không thành công. Vui lòng kiểm tra lại thông tin!";

                    ModelState.AddModelError("", errorMsg);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi kết nối Server API: {ex.Message}");
            }

            return View(model);
        }
        
        // 6. GET: /Account/GoogleLogin (BẮN KHÁCH SANG TRANG CHỌN TÀI KHOẢN CỦA GOOGLE)
        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl)
        {
            // Lưu lại URL hiện tại của khách vào TempData để sau khi login xong quay về
            TempData["ReturnUrl"] = returnUrl ?? "/Home/Index";
            
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // 7. GET: /Account/GoogleResponse (NHẬN THÔNG TIN TỪ GOOGLE TRẢ VỀ VÀ ĐỒNG BỘ SANG API)
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            // Đọc thông tin mã hóa từ Cookie Google trả về cho MVC
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            // Bốc tách Email và Họ tên của khách hàng
            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Không thể lấy thông tin Email bảo mật từ tài khoản Google này.");
                return RedirectToAction("Login");
            }

            try
            {
                // BẮN THÔNG TIN SANG API ĐỂ SQL SERVER KIỂM TRA/KHỞI TẠO TÀI KHOẢN
                var googleRequestBody = new { Email = email, HoTen = name ?? "Google User" };
                var response = await _httpClient.PostAsJsonAsync(_apiUrl + "GoogleLogin", googleRequestBody);

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    
                    if (apiResult != null && apiResult.ContainsKey("username"))
                    {
                        string systemUsername = apiResult["username"].ToString()!;

                        // 1. Cấp Session danh tính cho người dùng chạy xuyên suốt Website MVC
                        HttpContext.Session.SetString("UserSession", systemUsername);

                        // ========================================================
                        // LƯU AVATAR VÀO SESSION KHI ĐĂNG NHẬP BẰNG GOOGLE
                        // ========================================================
                        if (apiResult.ContainsKey("avatar") && apiResult["avatar"] != null)
                        {
                            HttpContext.Session.SetString("UserAvatar", apiResult["avatar"].ToString()!);
                        }

                        // 2. PHỤC HỒI GIỎ HÀNG THẦN TỐC: Đổ lại dữ liệu cũ vào Session "GioHang" của bạn
                        if (apiResult.ContainsKey("dataCart") && apiResult["dataCart"] != null)
                        {
                            string oldCartJson = apiResult["dataCart"].ToString()!;
                            if (!string.IsNullOrEmpty(oldCartJson) && oldCartJson.StartsWith("["))
                            {
                                HttpContext.Session.SetString("GioHang", oldCartJson);
                            }
                        }
                    }
                    // Đọc URL, dùng ?? để đảm bảo nếu TempData bị mất (do Timeout hoặc lỗi) thì luôn về trang chủ
                    string returnUrl = TempData["ReturnUrl"]?.ToString() ?? "/Home/Index";
                    
                    // Xóa cookie đệm nếu cần
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    return Redirect(returnUrl);
                }
                else
                {
                    TempData["ErrorMessage"] = "Đăng nhập bằng Google thất bại tại hệ thống tổng kho API.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi kết nối API: {ex.Message}";
            }

            return RedirectToAction("Login");
        }
    }
}