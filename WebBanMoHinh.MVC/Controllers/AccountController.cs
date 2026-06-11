using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
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
        private readonly string _apiUrl;

        public AccountController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // Đọc URL từ appsettings.json, thiết lập fallback mặc định nếu không tìm thấy
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        // 1. GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        // 2. POST: /Account/Login (XỬ LÝ ĐĂNG NHẬP & GỘP GIỎ HÀNG)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var requestBody = new { Username = model.TenDangNhap, Password = model.MatKhau };
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}TaiKhoans/DangNhap", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc phản hồi từ API để lấy thêm trường Avatar
                    var apiResult = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

                    // 1. Lưu Session Đăng nhập của Pilot trên MVC
                    HttpContext.Session.SetString("UserSession", model.TenDangNhap);

                    // LƯU AVATAR VÀO SESSION KHI ĐĂNG NHẬP THƯỜNG
                    if (apiResult != null && apiResult.ContainsKey("avatar") && apiResult["avatar"] != null)
                    {
                        HttpContext.Session.SetString("UserAvatar", apiResult["avatar"].ToString()!);
                    }

                    // 2. ĐỒNG BỘ VÀ GỘP (MERGE) GIỎ HÀNG VÃNG LAI VÀO TÀI KHOẢN
                    try
                    {
                        string? guestCartJson = HttpContext.Session.GetString("GioHang");
                        var guestCart = string.IsNullOrEmpty(guestCartJson) 
                            ? new List<CartItemViewModel>() 
                            : JsonSerializer.Deserialize<List<CartItemViewModel>>(guestCartJson);

                        var cartResponse = await _httpClient.GetAsync($"{_apiUrl}Cart/GetCart/{model.TenDangNhap}");
                        if (cartResponse.IsSuccessStatusCode)
                        {
                            var cartData = await cartResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                            
                            // NẾU DATABASE ĐÃ CÓ GIỎ HÀNG CŨ -> GỘP CHÚNG LẠI
                            if (cartData != null && cartData.ContainsKey("cartJson") && !string.IsNullOrEmpty(cartData["cartJson"]))
                            {
                                var dbCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData["cartJson"]) ?? new List<CartItemViewModel>();

                                if (guestCart != null && guestCart.Any())
                                {
                                    foreach (var item in guestCart)
                                    {
                                        var existItem = dbCart.FirstOrDefault(x => x.MaSp == item.MaSp);
                                        if (existItem != null) existItem.SoLuong += item.SoLuong;
                                        else dbCart.Add(item);
                                    }
                                }
                                
                                string mergedCartJson = JsonSerializer.Serialize(dbCart);
                                HttpContext.Session.SetString("GioHang", mergedCartJson);
                                await _httpClient.PostAsJsonAsync($"{_apiUrl}Cart/SaveCart", new { Username = model.TenDangNhap, CartJson = mergedCartJson });
                            }
                            // NẾU DATABASE RỖNG NHƯNG KHÁCH VÃNG LAI CÓ HÀNG -> LƯU THẲNG LÊN DB
                            else if (guestCart != null && guestCart.Any())
                            {
                                await _httpClient.PostAsJsonAsync($"{_apiUrl}Cart/SaveCart", new { Username = model.TenDangNhap, CartJson = guestCartJson });
                            }
                        }
                    }
                    catch { }

                    // Điều hướng sau khi login thành công (Đá ngược về trang Thanh toán nếu có)
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
                    await _httpClient.PostAsJsonAsync($"{_apiUrl}Cart/SaveCart", saveBody);
                }
                catch { }
            }

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

                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}TaiKhoans/DangKy", requestBody);

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
            TempData["ReturnUrl"] = returnUrl ?? "/Home/Index";
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // 7. GET: /Account/GoogleResponse (NHẬN THÔNG TIN TỪ GOOGLE TRẢ VỀ VÀ ĐỒNG BỘ SANG API)
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

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
                var googleRequestBody = new { Email = email, HoTen = name ?? "Google User" };
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}TaiKhoans/GoogleLogin", googleRequestBody);

                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                    
                    if (apiResult != null && apiResult.ContainsKey("username"))
                    {
                        string systemUsername = apiResult["username"].ToString()!;

                        HttpContext.Session.SetString("UserSession", systemUsername);

                        if (apiResult.ContainsKey("avatar") && apiResult["avatar"] != null)
                        {
                            HttpContext.Session.SetString("UserAvatar", apiResult["avatar"].ToString()!);
                        }

                        // GỘP GIỎ HÀNG CHO GOOGLE LOGIN
                        try
                        {
                            string? guestCartJson = HttpContext.Session.GetString("GioHang");
                            var guestCart = string.IsNullOrEmpty(guestCartJson) 
                                ? new List<CartItemViewModel>() 
                                : JsonSerializer.Deserialize<List<CartItemViewModel>>(guestCartJson);

                            var cartResponse = await _httpClient.GetAsync($"{_apiUrl}Cart/GetCart/{systemUsername}");
                            if (cartResponse.IsSuccessStatusCode)
                            {
                                var cartData = await cartResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                                
                                if (cartData != null && cartData.ContainsKey("cartJson") && !string.IsNullOrEmpty(cartData["cartJson"]))
                                {
                                    var dbCart = JsonSerializer.Deserialize<List<CartItemViewModel>>(cartData["cartJson"]) ?? new List<CartItemViewModel>();

                                    if (guestCart != null && guestCart.Any())
                                    {
                                        foreach (var item in guestCart)
                                        {
                                            var existItem = dbCart.FirstOrDefault(x => x.MaSp == item.MaSp);
                                            if (existItem != null) existItem.SoLuong += item.SoLuong;
                                            else dbCart.Add(item);
                                        }
                                    }
                                    
                                    string mergedCartJson = JsonSerializer.Serialize(dbCart);
                                    HttpContext.Session.SetString("GioHang", mergedCartJson);
                                    await _httpClient.PostAsJsonAsync($"{_apiUrl}Cart/SaveCart", new { Username = systemUsername, CartJson = mergedCartJson });
                                }
                                else if (guestCart != null && guestCart.Any())
                                {
                                    await _httpClient.PostAsJsonAsync($"{_apiUrl}Cart/SaveCart", new { Username = systemUsername, CartJson = guestCartJson });
                                }
                            }
                        }
                        catch { }
                    }

                    string returnUrl = TempData["ReturnUrl"]?.ToString() ?? "/Home/Index";
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

        // 8. GET: /Account/ForgotPassword (MỞ GIAO DIỆN QUÊN MẬT KHẨU)
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 9. POST: /Account/ForgotPassword (GỬI EMAIL SANG API ĐỂ XỬ LÝ)
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Đóng gói Email gửi sang Endpoint "QuenMatKhau" của API
                var requestBody = new { Email = model.Email };
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}TaiKhoans/QuenMatKhau", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    // Nếu API báo thành công, hiện thông báo xanh và đá về trang Đăng nhập
                    TempData["SuccessMessage"] = "Hệ thống đã gửi mã khôi phục! Vui lòng kiểm tra hộp thư Email của bạn.";
                    return RedirectToAction("Login");
                }
                else
                {
                    // Nếu API báo lỗi (VD: Email không tồn tại), hiện thông báo đỏ
                    var errorResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    string errorMsg = errorResult != null && errorResult.ContainsKey("message") 
                        ? errorResult["message"] 
                        : "Email này chưa được liên kết với bất kỳ tài khoản Pilot nào trên hệ thống!";

                    TempData["ErrorMessage"] = errorMsg;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi kết nối đến Tổng kho API: {ex.Message}";
            }

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            // Kiểm tra dữ liệu cơ bản
            if (model.MatKhauMoi != model.NhapLaiMatKhauMoi)
                return Json(new { success = false, message = "Mật khẩu mới không trùng khớp!" });

            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username)) 
                return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn!" });

            try
            {
                var requestData = new { TenDangNhap = username, MatKhauCu = model.MatKhauCu, MatKhauMoi = model.MatKhauMoi };
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}TaiKhoans/CapNhatProfile", requestData);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Thay đổi mật khẩu thành công!" });
                }
                else
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    return Json(new { success = false, message = errorResult?.GetValueOrDefault("message") ?? "Đổi mật khẩu thất bại!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi kết nối: " + ex.Message });
            }
        }
    }
}