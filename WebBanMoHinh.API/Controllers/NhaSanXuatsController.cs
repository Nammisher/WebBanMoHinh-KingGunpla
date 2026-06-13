using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhaSanXuatsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        public NhaSanXuatsController(WebBanMoHinhContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetNhaSanXuats()
        {
            // SỬA Ở ĐÂY: Cắt vòng lặp JSON
            return await _context.NhaSanXuat
                .Select(x => new { x.MaNsx, x.TenNsx, x.QuocGia })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<NhaSanXuat>> PostNhaSanXuat(NhaSanXuat nsx)
        {
            _context.NhaSanXuat.Add(nsx);
            await _context.SaveChangesAsync();
            return Ok(nsx);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNhaSanXuat(int id, NhaSanXuat nsx)
        {
            if (id != nsx.MaNsx) return BadRequest(new { message = "ID không hợp lệ!" });
            _context.Entry(nsx).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhaSanXuat(int id)
        {
            var nsx = await _context.NhaSanXuat.FindAsync(id);
            if (nsx == null) return NotFound();

            bool isUsed = await _context.SanPham.AnyAsync(sp => sp.MaNsx == id && sp.DaXoa != true);
            if (isUsed) return BadRequest(new { message = "Không thể xóa! Đang có Mô hình thuộc Hãng này." });

            _context.NhaSanXuat.Remove(nsx);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công!" });
        }
    }
}