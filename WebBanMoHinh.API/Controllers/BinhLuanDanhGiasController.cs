using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BinhLuanDanhGiasController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;

        public BinhLuanDanhGiasController(WebBanMoHinhContext context)
        {
            _context = context;
        }

        // 1. GET: api/BinhLuanDanhGias/SanPham/1 (Lấy toàn bộ đánh giá của 1 mô hình)
        [HttpGet("SanPham/{maSp}")]
        public async Task<ActionResult<IEnumerable<BinhLuanDanhGia>>> GetBinhLuanCuaSanPham(int maSp)
        {
            var binhLuans = await _context.BinhLuanDanhGia
                .Where(b => b.MaSp == maSp)
                .OrderByDescending(b => b.NgayBinhLuan)
                .ToListAsync();

            return Ok(binhLuans);
        }

        // 2. POST: api/BinhLuanDanhGias (Khách hàng gửi đánh giá mới)
        [HttpPost]
        public async Task<IActionResult> GuiBinhLuan(BinhLuanDanhGia binhLuan)
        {
            binhLuan.NgayBinhLuan = DateTime.Now;

            if (binhLuan.SoSao < 1 || binhLuan.SoSao > 5)
            {
                return BadRequest(new { message = "Số sao đánh giá phải từ 1 đến 5!" });
            }

            _context.BinhLuanDanhGia.Add(binhLuan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng đánh giá thành công!", data = binhLuan });
        }
    }
}