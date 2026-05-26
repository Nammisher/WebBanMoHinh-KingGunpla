using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;
using WebBanMoHinh.API.Services;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangsController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        private readonly IOrderFacade _orderFacade; // Dùng Facade ở đây

        // Tiêm cả Context và Facade vào để dùng cho các hàm tương ứng
        public DonHangsController(WebBanMoHinhContext context, IOrderFacade orderFacade)
        {
            _context = context;
            _orderFacade = orderFacade;
        }

        // 1. POST: api/DonHangs (Đặt hàng qua Facade Pattern)
        [HttpPost]
        public async Task<IActionResult> DatHang(DonHang donHang)
        {
            if (donHang.ChiTietDonHang == null || !donHang.ChiTietDonHang.Any())
            {
                return BadRequest(new { message = "Đơn hàng phải có ít nhất một sản phẩm!" });
            }

            // Gọi trực tiếp qua biến private _orderFacade chuẩn quy trình
            var result = await _orderFacade.PlaceOrderAsync(donHang);
            
            if (!result.Success) return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, maDonHang = result.OrderId });
        }

        // 2. GET: api/DonHangs (Xem toàn bộ đơn hàng - Admin)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonHang>>> GetDonHangs()
        {
            return await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();
        }

        // 3. GET: api/DonHangs/LichSu/demo12 (Xem lịch sử mua hàng)
        [HttpGet("LichSu/{username}")]
        public async Task<ActionResult<IEnumerable<DonHang>>> GetLichSuDonHang(string username)
        {
            var lichSu = await _context.DonHang
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(c => c.MaSpNavigation) // DÒNG MỚI: Kéo thêm thông tin Sản Phẩm
                .Where(d => d.TenDangNhap == username)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return Ok(lichSu);
        }

        // 4. PUT: api/DonHangs/DoiTrangThai/5 (Duyệt đơn)
        [HttpPut("DoiTrangThai/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string trangThaiMoi)
        {
            var donHang = await _context.DonHang.FindAsync(id);
            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            donHang.TrangThaiDonHang = trangThaiMoi;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái đơn hàng thành công!" });
        }
    }
}