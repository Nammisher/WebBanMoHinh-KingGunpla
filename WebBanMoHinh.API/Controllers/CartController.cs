using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;

        public CartController(WebBanMoHinhContext context)
        {
            _context = context;
        }

        // 1. API LẤY GIỎ HÀNG TỪ BẢNG GIOHANG: GET /api/Cart/GetCart/admin
        [HttpGet("GetCart/{username}")]
        public async Task<IActionResult> GetCart(string username)
        {
            // Quét thẳng vào bảng GioHang
            var gioHang = await _context.GioHang.FirstOrDefaultAsync(g => g.TenDangNhap == username);
            
            // Nếu chưa từng có giỏ hàng, trả về chuỗi rỗng để MVC tự gộp, KHÔNG trả lỗi NotFound
            if (gioHang == null) return Ok(new { cartJson = "" });

            return Ok(new { cartJson = gioHang.CartJson });
        }

        // 2. API LƯU GIỎ HÀNG VÀO BẢNG GIOHANG: POST /api/Cart/SaveCart
        [HttpPost("SaveCart")]
        public async Task<IActionResult> SaveCart([FromBody] SaveCartDTO request)
        {
            var gioHang = await _context.GioHang.FirstOrDefaultAsync(g => g.TenDangNhap == request.Username);

            if (gioHang == null)
            {
                // Khách này chưa có record giỏ hàng -> Tạo mới trong bảng GioHang
                gioHang = new GioHang 
                { 
                    TenDangNhap = request.Username, 
                    CartJson = request.CartJson 
                };
                _context.GioHang.Add(gioHang);
            }
            else
            {
                // Đã có rồi -> Ghi đè chuỗi JSON mới
                gioHang.CartJson = request.CartJson;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Lưu giỏ hàng thành công!" });
        }
    }

    // DTO hứng dữ liệu từ MVC gửi lên
    public class SaveCartDTO
    {
        public string Username { get; set; } = null!;
        public string CartJson { get; set; } = null!;
    }
}