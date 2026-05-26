using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json; 
using Newtonsoft.Json;       
using WebBanMoHinh.MVC.Models;
using Microsoft.AspNetCore.Http;

namespace WebBanMoHinh.MVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly HttpClient _httpClient;
        
        public CheckoutController(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        // ========================================================
        // ĐÃ SỬA: HÀM INDEX HIỂN THỊ TRANG THANH TOÁN KÈM GIỎ HÀNG
        // ========================================================
        [HttpGet]
        public IActionResult Index()
        {
            // 1. Lấy chuỗi JSON giỏ hàng từ Session
            string? cartJson = HttpContext.Session.GetString("GioHang");
            var cartItems = new List<CartItemViewModel>();

            // 2. Nếu có giỏ hàng, chuyển JSON thành List<CartItemViewModel>
            if (!string.IsNullOrEmpty(cartJson))
            {
                cartItems = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>();
            }

            // 3. Nạp vào ViewBag để truyền sang Index.cshtml
            ViewBag.Cart = cartItems;

            return View(new CheckoutViewModel()); 
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToan(CheckoutViewModel model)
        {
            string? cartJson = HttpContext.Session.GetString("GioHang");
            if (string.IsNullOrEmpty(cartJson)) return RedirectToAction("Index", "Cart");

            var cartItems = JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
            if (cartItems == null || !cartItems.Any()) return RedirectToAction("Index", "Cart");

            var orderItems = cartItems.Select(x => new {
                MaSp = x.MaSp,
                SoLuong = x.SoLuong,
                GiaBan = x.GiaBan 
            }).ToList();

            var orderData = new {
                TenDangNhap = HttpContext.Session.GetString("UserSession"),
                model.DiaChiGiaoHang,
                model.SoDienThoai,
                model.PhuongThucThanhToan,
                Items = orderItems
            };

            try 
            {
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5298/api/Checkout/DatHang", orderData);
                
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string maDon = responseData?.maDonHang?.ToString() ?? "UNKNOWN";
                    
                    decimal totalAmount = cartItems.Sum(x => x.GiaBan * x.SoLuong);
                    string username = HttpContext.Session.GetString("UserSession") ?? "KhachHang";
                    
                    HttpContext.Session.Remove("GioHang"); 

                    // LOGIC BẺ LÁI ĐIỀU HƯỚNG DỰA TRÊN PHƯƠNG THỨC THANH TOÁN
                    string pttt = model.PhuongThucThanhToan?.ToUpper() ?? "";
                    
                    if (pttt.Contains("ONLINE") || pttt.Contains("CHUYỂN KHOẢN") || pttt.Contains("CHUYEN KHOAN"))
                    {
                        return RedirectToAction("ThanhToanOnline", "Checkout", new { 
                            orderId = maDon, 
                            amount = totalAmount,
                            username = username
                        });
                    }
                    
                    return RedirectToAction("Success", "Checkout");
                }
                else
                {
                    dynamic? errorResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string apiError = errorResponse?.message ?? "Lỗi không xác định từ Database.";
                    ModelState.AddModelError("", $"Lỗi từ hệ thống: {apiError}");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi code/mạng: {ex.Message}");
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult ThanhToanOnline(string orderId, decimal amount, string username)
        {
            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            
            string cleanUsername = username?.Replace(" ", "") ?? "KhachHang";
            ViewBag.TransferContent = $"{cleanUsername} thanh toan don {orderId}";
            
            return View();
        }

        [HttpGet]
        public IActionResult Success() => View();
    }
}