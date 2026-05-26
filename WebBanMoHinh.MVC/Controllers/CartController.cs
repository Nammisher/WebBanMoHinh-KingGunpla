using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebBanMoHinh.MVC.Models;

namespace WebBanMoHinh.MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public CartController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration.GetValue<string>("BackendApiUrl") ?? "http://localhost:5298/api/";
        }

        private List<CartItemViewModel> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString("GioHang");
            if (string.IsNullOrEmpty(sessionData)) return new List<CartItemViewModel>();
            return JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionData) ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetString("GioHang", JsonSerializer.Serialize(cart));
        }

        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int maSp, int soLuong)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MaSp == maSp);

            if (item != null)
            {
                item.SoLuong += soLuong; 
            }
            else
            {
                var response = await _httpClient.GetAsync(_apiUrl + $"SanPhams/{maSp}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var sp = JsonSerializer.Deserialize<SanPhamViewModel>(jsonData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (sp != null)
                    {
                        cart.Add(new CartItemViewModel
                        {
                            MaSp = sp.MaSp,
                            TenSp = sp.TenSp,
                            HinhAnhChinh = sp.HinhAnhChinh,
                            
                            // ĐÃ FIX: Ưu tiên lấy Giá Giảm (nếu có và lớn hơn 0), nếu không mới lấy Giá Bán gốc
                            GiaBan = (sp.GiaGiam.HasValue && sp.GiaGiam > 0) ? sp.GiaGiam.Value : (sp.GiaBan ?? 0),
                            
                            SoLuong = soLuong
                        });
                    }
                }
            }

            SaveCart(cart);
            return RedirectToAction("Index"); 
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.MaSp == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
    }
}