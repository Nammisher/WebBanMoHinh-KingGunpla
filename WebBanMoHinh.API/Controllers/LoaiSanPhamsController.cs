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

        public LoaiSanPhamsController(WebBanMoHinhContext context)
        {
            _context = context;
        }

        // GET: api/LoaiSanPhams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoaiSanPham>>> GetLoaiSanPhams()
        {
            return await _context.LoaiSanPham.ToListAsync();
        }

        // POST: api/LoaiSanPhams
        [HttpPost]
        public async Task<ActionResult<LoaiSanPham>> PostLoaiSanPham(LoaiSanPham loai)
        {
            _context.LoaiSanPham.Add(loai);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLoaiSanPhams), new { id = loai.MaLoai }, loai);
        }
    }
}