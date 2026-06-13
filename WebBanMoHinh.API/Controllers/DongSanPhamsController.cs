using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DongSanPhamsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        public DongSanPhamsController(WebBanMoHinhContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDongSanPhams()
        {
            // SỬA Ở ĐÂY: Cắt vòng lặp JSON
            return await _context.DongSanPham
                .Select(x => new { x.MaDong, x.TenDong, x.MoTa })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<DongSanPham>> PostDongSanPham(DongSanPham dong)
        {
            _context.DongSanPham.Add(dong);
            await _context.SaveChangesAsync();
            return Ok(dong);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDongSanPham(int id, DongSanPham dong)
        {
            if (id != dong.MaDong) return BadRequest(new { message = "ID không hợp lệ!" });
            _context.Entry(dong).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDongSanPham(int id)
        {
            var dong = await _context.DongSanPham.FindAsync(id);
            if (dong == null) return NotFound();

            bool isUsed = await _context.SanPham.AnyAsync(sp => sp.MaDong == id && sp.DaXoa != true);
            if (isUsed) return BadRequest(new { message = "Không thể xóa! Đang có Mô hình thuộc Dòng này." });

            _context.DongSanPham.Remove(dong);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }
    }
}