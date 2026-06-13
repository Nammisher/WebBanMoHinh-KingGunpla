using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminSanPhamsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        public AdminSanPhamsController(WebBanMoHinhContext context) { _context = context; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSanPhams()
        {
            var list = await _context.SanPham
                .Include(s => s.MaLoaiNavigation)
                .Include(s => s.MaDongNavigation)
                .Include(s => s.MaNsxNavigation)
                .Where(s => s.DaXoa == false || s.DaXoa == null)
                .OrderByDescending(s => s.NgayNhap)
                .Select(s => new {
                    s.MaSp, s.TenSp, s.GiaBan, s.SoLuongTon, s.HinhAnhChinh, s.IsHot,
                    MaLoai = s.MaLoai, TenLoai = s.MaLoaiNavigation != null ? s.MaLoaiNavigation.TenLoai : "",
                    MaDong = s.MaDong, TenDong = s.MaDongNavigation != null ? s.MaDongNavigation.TenDong : "",
                    MaNsx = s.MaNsx, TenNsx = s.MaNsxNavigation != null ? s.MaNsxNavigation.TenNsx : "",
                    s.TiLe, s.KichThuoc, s.ChatLieu, s.MoTa
                })
                .ToListAsync();
            return Ok(list);
        }

        // SỬA Ở ĐÂY: Dùng SanPhamInputDto thay vì SanPham gốc
        [HttpPost]
        public async Task<IActionResult> PostSanPham([FromBody] SanPhamInputDto spInfo)
        {
            var sp = new SanPham {
                TenSp = spInfo.TenSp,
                GiaBan = spInfo.GiaBan,
                SoLuongTon = spInfo.SoLuongTon,
                MaLoai = spInfo.MaLoai,
                MaDong = spInfo.MaDong,
                MaNsx = spInfo.MaNsx,
                TiLe = spInfo.TiLe,
                KichThuoc = spInfo.KichThuoc,
                ChatLieu = spInfo.ChatLieu,
                MoTa = spInfo.MoTa,
                IsHot = spInfo.IsHot,
                HinhAnhChinh = spInfo.HinhAnhChinh,
                NgayNhap = DateTime.Now,
                DaXoa = false,
                Version = Array.Empty<byte>() // Bypass lỗi Required, Database sẽ tự sinh ra mã này
            };
            
            _context.SanPham.Add(sp);
            await _context.SaveChangesAsync();
            return Ok(sp);
        }

        // SỬA Ở ĐÂY: Dùng SanPhamInputDto
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSanPham(int id, [FromBody] SanPhamInputDto spInfo)
        {
            var sp = await _context.SanPham.FindAsync(id);
            if (sp == null) return NotFound(new { message = "Không tìm thấy sản phẩm!" });

            sp.TenSp = spInfo.TenSp;
            sp.GiaBan = spInfo.GiaBan;
            sp.SoLuongTon = spInfo.SoLuongTon;
            sp.MaLoai = spInfo.MaLoai;
            sp.MaDong = spInfo.MaDong;
            sp.MaNsx = spInfo.MaNsx;
            sp.TiLe = spInfo.TiLe;
            sp.KichThuoc = spInfo.KichThuoc;
            sp.ChatLieu = spInfo.ChatLieu;
            sp.MoTa = spInfo.MoTa;
            sp.IsHot = spInfo.IsHot;
            
            if (!string.IsNullOrEmpty(spInfo.HinhAnhChinh)) sp.HinhAnhChinh = spInfo.HinhAnhChinh;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSanPham(int id)
        {
            var sp = await _context.SanPham.FindAsync(id);
            if (sp == null) return NotFound();

            sp.DaXoa = true; 
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã chuyển sản phẩm vào thùng rác!" });
        }
    }

    // DTO hứng dữ liệu sạch từ MVC gửi qua (Tránh lỗi Version)
    public class SanPhamInputDto
    {
        public string TenSp { get; set; } = null!;
        public decimal GiaBan { get; set; }
        public int? SoLuongTon { get; set; }
        public string? HinhAnhChinh { get; set; }
        public string? MoTa { get; set; }
        public string? TiLe { get; set; }
        public string? KichThuoc { get; set; }
        public string? ChatLieu { get; set; }
        public int? MaLoai { get; set; }
        public int? MaDong { get; set; }
        public int? MaNsx { get; set; }
        public bool? IsHot { get; set; }
    }
}