using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;
using WebBanMoHinh.API.Services;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        private readonly ISanPhamService _sanPhamService; // Đã thêm dấu cách chuẩn chỉnh ở đây

        public SanPhamsController(WebBanMoHinhContext context, ISanPhamService sanPhamService)
        {
            _context = context;
            _sanPhamService = sanPhamService;
        }

        // 1. GET: api/SanPhams (Lấy toàn bộ - Chạy qua Decorator Pattern để ghi log)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SanPham>>> GetSanPhams()
        {
            var products = await _sanPhamService.GetAllActiveProductsAsync();
            return Ok(products);
        }

        // 2. GET: api/SanPhams/5 (Xem chi tiết)
        [HttpGet("{id}")]
        public async Task<ActionResult<SanPham>> GetSanPham(int id)
        {
            // DÙNG INCLUDE ĐỂ KÉO THEO DANH SÁCH BÌNH LUẬN TỪ DATABASE LÊN
            var sanPham = await _context.SanPham
                .Include(sp => sp.BinhLuanDanhGia) 
                .FirstOrDefaultAsync(sp => sp.MaSp == id);

            if (sanPham == null || sanPham.DaXoa == true)
            {
                return NotFound(new { message = "Không tìm thấy mô hình!" });
            }

            return sanPham;
        }

        // 3. POST: api/SanPhams (Thêm mới)
        [HttpPost]
        public async Task<ActionResult<SanPham>> PostSanPham(SanPham sanPham)
        {
            if (sanPham.NgayNhap == null) sanPham.NgayNhap = DateTime.Now;
            sanPham.DaXoa = false;

            _context.SanPham.Add(sanPham);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSanPham), new { id = sanPham.MaSp }, sanPham);
        }

        // 4. PUT: api/SanPhams/5 (Sửa đổi)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSanPham(int id, SanPham sanPham)
        {
            if (id != sanPham.MaSp) return BadRequest(new { message = "ID không trùng khớp!" });

            _context.Entry(sanPham).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SanPham.Any(e => e.MaSp == id)) return NotFound();
                else throw;
            }

            return Ok(new { message = "Cập nhật sản phẩm thành công!", data = sanPham });
        }

        // 5. DELETE: api/SanPhams/5 (Xóa mềm)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSanPham(int id)
        {
            var sanPham = await _context.SanPham.FindAsync(id);
            if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm!" });

            sanPham.DaXoa = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa sản phẩm thành công (Xóa mềm)!" });
        }

        // 6. CẬP NHẬT: API LỌC SẢN PHẨM (Hỗ trợ Search, Thương hiệu, Mã dòng, Mức giá, Sắp xếp)
        [HttpGet("LocSanPham")]
        public async Task<ActionResult<IEnumerable<SanPham>>> LocSanPham(
            [FromQuery] string? search,
            [FromQuery] int? maNsx,
            [FromQuery] int? maDong, // <--- ĐÃ THÊM THAM SỐ LỌC THEO DÒNG SẢN PHẨM
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy)
        {
            // 1. Khởi tạo Query lấy các sản phẩm đang bán (chưa bị xóa mềm)
            var query = _context.SanPham.AsQueryable().Where(sp => sp.DaXoa != true);

            // 2. Lọc theo từ khóa tìm kiếm (Tìm kiếm thông minh bất chấp dấu cách)
            if (!string.IsNullOrEmpty(search))
            {
                // Tách chuỗi người dùng nhập thành các từ riêng lẻ (loại bỏ các dấu cách thừa)
                var keywords = search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Ép Entity Framework tạo câu lệnh SQL tìm kiếm chứa TẤT CẢ các từ khóa
                foreach (var word in keywords)
                {
                    query = query.Where(sp => sp.TenSp.Contains(word));
                }
            }

            // 3. Lọc theo Hãng sản xuất (Ví dụ: 2 là Bandai, 5 là Motor Nuclear...)
            if (maNsx.HasValue)
            {
                query = query.Where(sp => sp.MaNsx == maNsx);
            }

            // 4. LỌC THEO DÒNG SẢN PHẨM (Ví dụ: 1 = Gundam, 4 = DX Sentai...)
            if (maDong.HasValue)
            {
                query = query.Where(sp => sp.MaDong == maDong);
            }

            // 5. Lọc theo Mức giá (Logic thông minh: Ưu tiên lấy Giá Giảm để so sánh nếu đang có Sale)
            if (minPrice.HasValue)
            {
                query = query.Where(sp => (sp.GiaGiam != null && sp.GiaGiam > 0 ? sp.GiaGiam : sp.GiaBan) >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(sp => (sp.GiaGiam != null && sp.GiaGiam > 0 ? sp.GiaGiam : sp.GiaBan) <= maxPrice);
            }

            // 6. Sắp xếp (Sort) theo giao diện người dùng chọn
            query = sortBy switch
            {
                "az" => query.OrderBy(sp => sp.TenSp),
                "za" => query.OrderByDescending(sp => sp.TenSp),
                "price_asc" => query.OrderBy(sp => sp.GiaGiam != null && sp.GiaGiam > 0 ? sp.GiaGiam : sp.GiaBan),
                "price_desc" => query.OrderByDescending(sp => sp.GiaGiam != null && sp.GiaGiam > 0 ? sp.GiaGiam : sp.GiaBan),
                "newest" => query.OrderByDescending(sp => sp.NgayNhap),
                "oldest" => query.OrderBy(sp => sp.NgayNhap),
                _ => query.OrderByDescending(sp => sp.MaSp) // Mặc định hiển thị đồ mới nhất lên đầu
            };

            // 7. Chốt sổ và lấy dữ liệu từ SQL Server lên
            var result = await query.ToListAsync();
            
            if (result == null || !result.Any())
            {
                // Trả về mảng rỗng để bên Frontend dễ xử lý hiển thị chữ "Không tìm thấy"
                return Ok(new List<SanPham>()); 
            }

            return Ok(result);
        }
    }
}