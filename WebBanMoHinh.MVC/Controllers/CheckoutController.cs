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
        // HÀM INDEX HIỂN THỊ TRANG THANH TOÁN KÈM GIỎ HÀNG
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

            // 3. Khởi tạo ViewModel và nạp Item giỏ hàng vào cột OrderItems bên phải giao diện
            var viewModel = new CheckoutViewModel();
            
            if (cartItems.Any())
            {
                viewModel.OrderItems = cartItems.Select(c => new OrderItemViewModel {
                    Image = c.HinhAnhChinh ?? "fas fa-box", 
                    ProductName = c.TenSp,
                    Quantity = c.SoLuong,
                    Price = c.GiaBan
                }).ToList();
                
                viewModel.SubTotal = cartItems.Sum(c => c.GiaBan * c.SoLuong);
                viewModel.Total = viewModel.SubTotal; // Tính tổng tiền (phí ship có thể cộng thêm sau)
            }

            return View(viewModel); 
        }

        // ========================================================
        // HÀM XỬ LÝ ĐẶT HÀNG (Đổi tên thành ProcessPayment cho khớp HTML)
        // ========================================================
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
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

            // 1. Dịch Phương thức thanh toán từ Id sang Chữ
            string phuongThuc = model.SelectedPaymentMethodId switch
            {
                2 => "Chuyển khoản ngân hàng",
                6 => "Thanh toán qua VNPay",
                _ => "Thanh toán khi nhận hàng (COD)"
            };

            // 2. Gom dữ liệu đúng chuẩn gửi API
            var orderData = new {
                TenDangNhap = HttpContext.Session.GetString("UserSession") ?? "KhachHang",
                DiaChiGiaoHang = model.CustomerInfo?.FullAddress ?? "",
                SoDienThoai = model.CustomerInfo?.PhoneNumber ?? "",
                PhuongThucThanhToan = phuongThuc,
                Items = orderItems
            };

            try 
            {
                // Gọi API tạo đơn hàng
                var response = await _httpClient.PostAsJsonAsync("http://localhost:5298/api/Checkout/DatHang", orderData);
                
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseString);
                    string maDon = responseData?.maDonHang?.ToString() ?? "UNKNOWN";
                    
                    decimal totalAmount = cartItems.Sum(x => x.GiaBan * x.SoLuong);
                    string username = HttpContext.Session.GetString("UserSession") ?? "KhachHang";
                    
                    // Xóa giỏ hàng sau khi đặt thành công
                    HttpContext.Session.Remove("GioHang"); 

                    // 3. LOGIC BẺ LÁI ĐIỀU HƯỚNG DỰA TRÊN PHƯƠNG THỨC THANH TOÁN
                    string pttt = phuongThuc.ToUpper();
                    
                    if (pttt.Contains("CHUYỂN KHOẢN") || pttt.Contains("VNPAY"))
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
                    
                    // Nếu lỗi, nạp lại giỏ hàng vào view để hiển thị
                    model.OrderItems = cartItems.Select(c => new OrderItemViewModel {
                        Image = c.HinhAnhChinh ?? "fas fa-box", ProductName = c.TenSp, Quantity = c.SoLuong, Price = c.GiaBan
                    }).ToList();
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