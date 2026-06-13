using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiSanPhamsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        public LoaiSanPhamsController(WebBanMoHinhContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLoaiSanPhams()
        {
            // SỬA Ở ĐÂY: Dùng Select để cắt đứt vòng lặp JSON
            return await _context.LoaiSanPham
                .Select(x => new { x.MaLoai, x.TenLoai, x.MoTa })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<LoaiSanPham>> PostLoaiSanPham(LoaiSanPham loai)
        {
            _context.LoaiSanPham.Add(loai);
            await _context.SaveChangesAsync();
            return Ok(loai);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLoaiSanPham(int id, LoaiSanPham loai)
        {
            if (id != loai.MaLoai) return BadRequest(new { message = "ID không hợp lệ!" });
            _context.Entry(loai).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoaiSanPham(int id)
        {
            var loai = await _context.LoaiSanPham.FindAsync(id);
            if (loai == null) return NotFound();

            bool isUsed = await _context.SanPham.AnyAsync(sp => sp.MaLoai == id && sp.DaXoa != true);
            if (isUsed) return BadRequest(new { message = "Không thể xóa! Đang có Mô hình thuộc Loại này." });

            _context.LoaiSanPham.Remove(loai);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }
    }
}