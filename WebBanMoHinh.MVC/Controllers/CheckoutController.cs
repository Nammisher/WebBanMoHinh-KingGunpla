using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json; 
using Newtonsoft.Json;       
using WebBanMoHinh.MVC.Models;
using Microsoft.AspNetCore.Http;
using WebBanMoHinh.MVC.Helpers; 

namespace WebBanMoHinh.MVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; 
        
        public CheckoutController(HttpClient httpClient, IConfiguration configuration) 
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

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

        [HttpGet]
        public IActionResult Index()
        {
            string? username = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout/Index" });
            }

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

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
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

            // 1. TÍNH PHÍ SHIP VÀ TỔNG TIỀN CHÍNH XÁC Ở ĐÂY
            // Nếu model.SelectedShippingMethodId == 1 (Giao tận nơi) thì phí 25.000đ, ngược lại nhận tại kho là 0đ
            decimal shippingFee = (model.SelectedShippingMethodId == 1) ? 25000 : 0; 
            decimal totalAmountWithShip = checkoutItems.Sum(x => x.GiaBan * x.SoLuong) + shippingFee;

            var orderData = new {
                TenDangNhap = HttpContext.Session.GetString("UserSession") ?? "KhachHang",
                DiaChiGiaoHang = model.CustomerInfo?.FullAddress ?? "",
                SoDienThoai = model.CustomerInfo?.PhoneNumber ?? "",
                PhuongThucThanhToan = phuongThuc,
                Items = orderItems,
                TongTien = totalAmountWithShip // 2. Bổ sung trường này gửi xuống API
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
                    
                    if (pttt.Contains("VNPAY"))
                    {
                        // Truyền totalAmountWithShip vào để VNPay thu đúng số tiền
                        string vnpayUrl = GenerateVnPayUrl(totalAmountWithShip, maDon);
                        return Redirect(vnpayUrl); 
                    }
                    else if (pttt.Contains("CHUYỂN KHOẢN"))
                    {
                        return RedirectToAction("ThanhToanOnline", "Checkout", new { orderId = maDon, amount = totalAmount, username = username });
                    }
                    
                    return RedirectToAction("Success", "Checkout");
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Backend API báo lỗi: {errorDetails}");
                    
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

        // HÀM ĐÃ SỬA: KHÔNG CÒN HARDCODE
        private string GenerateVnPayUrl(decimal amount, string orderId)
        {
            string tmnCode = _configuration["VnPay:TmnCode"] ?? "";
            string hashSecret = _configuration["VnPay:HashSecret"] ?? ""; 
            string baseUrl = _configuration["VnPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string returnUrl = _configuration["VnPay:ReturnUrl"] ?? "http://localhost:5208/Checkout/PaymentCallback";

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)amount * 100).ToString()); 
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1") ipAddress = "127.0.0.1";
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "ThanhToanDonHang_" + orderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); 
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", orderId + "_" + DateTime.Now.Ticks.ToString()); 

            return vnpay.CreateRequestUrl(baseUrl, hashSecret);
        }

        // HÀM ĐÃ SỬA: KHÔNG CÒN HARDCODE
        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var vnpayData = Request.Query;
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnpay.AddResponseData(key, value.ToString());
            }

            string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();
            string hashSecret = _configuration["VnPay:HashSecret"] ?? ""; 

            if (vnpay.ValidateSignature(vnp_SecureHash, hashSecret))
            {
                if (vnp_ResponseCode == "00") return RedirectToAction("Success", "Checkout");
                TempData["Error"] = "Thanh toán VNPay thất bại!";
                return RedirectToAction("Index", "Cart");
            }
            TempData["Error"] = "Lỗi xác thực chữ ký!";
            return RedirectToAction("Index", "Cart");
        }
    }
}