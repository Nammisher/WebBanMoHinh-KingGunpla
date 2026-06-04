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

        // HÀM MỚI: HỨNG CÁC SẢN PHẨM ĐƯỢC TICK CHỌN TỪ GIỎ HÀNG
        [HttpPost]
        public IActionResult PrepareCheckout(List<int> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                TempData["Error"] = "Vui lòng tick chọn ít nhất 1 mô hình để thanh toán!";
                return RedirectToAction("Index", "Cart");
            }

            HttpContext.Session.SetString("SelectedItems", string.Join(",", selectedItems));
            return RedirectToAction("Index");
        }

        // HÀM INDEX HIỂN THỊ TRANG THANH TOÁN (CHỈ HIỂN THỊ MÓN ĐÃ CHỌN)
        [HttpGet]
        public IActionResult Index()
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout/Index" });
            }

            // [FIX WARNING CS8604]: Thêm ?? new List... để đảm bảo biến này không bao giờ null
            string? cartJson = HttpContext.Session.GetString("GioHang");
            var allCartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItemViewModel>() 
                : (JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>());

            string? selectedStr = HttpContext.Session.GetString("SelectedItems");
            var selectedIds = string.IsNullOrEmpty(selectedStr) 
                ? new List<int>() 
                : selectedStr.Split(',').Select(int.Parse).ToList();

            var cartItems = allCartItems.Where(x => selectedIds.Contains(x.MaSp)).ToList();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            ViewBag.Cart = cartItems; 

            var viewModel = new CheckoutViewModel();
            viewModel.OrderItems = cartItems.Select(c => new OrderItemViewModel {
                Image = c.HinhAnhChinh ?? "fas fa-box", 
                ProductName = c.TenSp,
                Quantity = c.SoLuong,
                Price = c.GiaBan
            }).ToList();
            
            viewModel.SubTotal = cartItems.Sum(c => c.GiaBan * c.SoLuong);
            viewModel.Total = viewModel.SubTotal;

            return View(viewModel); 
        }

        // HÀM XỬ LÝ ĐẶT HÀNG (CHỈ XÓA NHỮNG MÓN ĐÃ TICK KHỎI GIỎ HÀNG)
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            // [FIX WARNING CS8604]: Tương tự ở đây
            string? cartJson = HttpContext.Session.GetString("GioHang");
            var allCartItems = string.IsNullOrEmpty(cartJson) 
                ? new List<CartItemViewModel>() 
                : (JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson) ?? new List<CartItemViewModel>());

            string? selectedStr = HttpContext.Session.GetString("SelectedItems");
            var selectedIds = string.IsNullOrEmpty(selectedStr) 
                ? new List<int>() 
                : selectedStr.Split(',').Select(int.Parse).ToList();

            var checkoutItems = allCartItems.Where(x => selectedIds.Contains(x.MaSp)).ToList();
            if (!checkoutItems.Any()) return RedirectToAction("Index", "Cart");

            var orderItems = checkoutItems.Select(x => new {
                MaSp = x.MaSp, SoLuong = x.SoLuong, GiaBan = x.GiaBan 
            }).ToList();

            string phuongThuc = model.SelectedPaymentMethodId switch {
                2 => "Chuyển khoản ngân hàng", 6 => "Thanh toán qua VNPay", _ => "Thanh toán khi nhận hàng (COD)"
            };

            var orderData = new {
                TenDangNhap = HttpContext.Session.GetString("UserSession") ?? "KhachHang",
                DiaChiGiaoHang = model.CustomerInfo?.FullAddress ?? "",
                SoDienThoai = model.CustomerInfo?.PhoneNumber ?? "",
                PhuongThucThanhToan = phuongThuc,
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
                    decimal totalAmount = checkoutItems.Sum(x => x.GiaBan * x.SoLuong);
                    string username = HttpContext.Session.GetString("UserSession") ?? "KhachHang";
                    
                    allCartItems.RemoveAll(x => selectedIds.Contains(x.MaSp));
                    HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(allCartItems));
                    HttpContext.Session.Remove("SelectedItems"); 

                    string pttt = phuongThuc.ToUpper();
                    if (pttt.Contains("CHUYỂN KHOẢN") || pttt.Contains("VNPAY"))
                    {
                        return RedirectToAction("ThanhToanOnline", "Checkout", new { orderId = maDon, amount = totalAmount, username = username });
                    }
                    return RedirectToAction("Success", "Checkout");
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi từ hệ thống khi tạo đơn!");
                    ViewBag.Cart = checkoutItems; 
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi kết nối: {ex.Message}");
                ViewBag.Cart = checkoutItems;
                return View("Index", model);
            }
        }

        [HttpGet]
        public IActionResult ThanhToanOnline(string orderId, decimal amount, string username)
        {
            ViewBag.OrderId = orderId; ViewBag.Amount = amount;
            string cleanUsername = username?.Replace(" ", "") ?? "KhachHang";
            ViewBag.TransferContent = $"{cleanUsername} thanh toan don {orderId}";
            return View();
        }

        [HttpGet]
        public IActionResult Success() => View();
    }
}