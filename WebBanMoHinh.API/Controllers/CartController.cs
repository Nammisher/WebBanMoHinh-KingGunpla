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

        // 1. API LẤY GIỎ HÀNG CŨ TỪ SQL SERVER: GET /api/Cart/GetCart/admin
        [HttpGet("GetCart/{username}")]
        public async Task<IActionResult> GetCart(string username)
        {
            // Duy quét bảng lưu chi tiết giỏ hàng của bạn dưới DB (ví dụ mình đặt là ChiTietGioHangs)
            // Nếu Duy chưa tạo bảng riêng, Duy có thể mượn cột DiaChi của bảng TaiKhoan để cất chuỗi JSON tại đây
            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.TenDangNhap == username);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy user" });

            return Ok(new { cartJson = taiKhoan.DiaChi });
        }

        // 2. API LƯU GIỎ HÀNG VÀO SQL SERVER: POST /api/Cart/SaveCart
        [HttpPost("SaveCart")]
        public async Task<IActionResult> SaveCart([FromBody] SaveCartDTO request)
        {
            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.TenDangNhap == request.Username);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy user" });

            // Lưu chuỗi giỏ hàng vào DB
            taiKhoan.DiaChi = request.CartJson;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã lưu giỏ hàng thành công!" });
        }
    }

    public class SaveCartDTO
    {
        public string Username { get; set; } = null!;
        public string? CartJson { get; set; }
    }
}