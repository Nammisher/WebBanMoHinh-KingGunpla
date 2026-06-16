using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminOrdersController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;
        public AdminOrdersController(WebBanMoHinhContext context) { _context = context; }

        // 1. LẤY DANH SÁCH TOÀN BỘ ĐƠN HÀNG (Hiển thị ở bảng trang Admin)
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.DonHang
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new {
                    d.MaDonHang,
                    d.TenDangNhap,
                    d.NgayDat,
                    d.TongTien,
                    d.PhiVanChuyen,
                    d.PhuongThucThanhToan,
                    d.TrangThaiDonHang,
                    d.MaVanDon 
                })
                .ToListAsync();
            return Ok(orders);
        }

        // 2. LẤY CHI TIẾT ĐƠN HÀNG (Dùng để hiển thị Modal và Xuất Bill In Ấn)
        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            // Lấy đơn hàng, kèm theo thông tin Khách Hàng (Tài Khoản) và Chi Tiết Sản Phẩm
            var order = await _context.DonHang
                .Include(d => d.TenDangNhapNavigation)
                .Include(d => d.ChiTietDonHang)
                    .ThenInclude(ct => ct.MaSpNavigation) // Join tiếp sang bảng Sản Phẩm để lấy Tên SP
                .FirstOrDefaultAsync(d => d.MaDonHang == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng!" });

            // Trả về JSON được format gọn gàng cho giao diện dễ xử lý
            var result = new {
                MaDonHang = order.MaDonHang,
                TenDangNhap = order.TenDangNhap,
                HoTenKhach = order.TenDangNhapNavigation?.HoTen ?? order.TenDangNhap,
                NgayDat = order.NgayDat,
                TongTien = order.TongTien,
                DiaChiGiaoHang = order.DiaChiGiaoHang,
                SoDienThoai = order.SoDienThoai,
                TrangThaiDonHang = order.TrangThaiDonHang,
                PhuongThucThanhToan = order.PhuongThucThanhToan,
                GhiChu = order.GhiChu,
                MaVanDon = order.MaVanDon,
                PhiVanChuyen = order.PhiVanChuyen ?? 0, // Nếu null thì trả về 0 để tránh lỗi khi tính tổng tiền cuối cùng
                // Mảng danh sách các sản phẩm khách đã mua trong bill
                Items = order.ChiTietDonHang.Select(ct => new {
                    MaSp = ct.MaSp,
                    TenSp = ct.MaSpNavigation?.TenSp ?? "Sản phẩm không xác định",
                    SoLuong = ct.SoLuong,
                    DonGia = ct.DonGia,
                    ThanhTien = (ct.SoLuong ?? 0) * (ct.DonGia ?? 0)
                }).ToList()
            };

            return Ok(result);
        }

        // 3. CẬP NHẬT TRẠNG THÁI & MÃ VẬN ĐƠN (Cập nhật thủ công)
        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderDto dto)
        {
            var order = await _context.DonHang.FindAsync(id);
            if (order == null) return NotFound(new { message = "Lỗi: Đơn hàng không tồn tại." });

            order.TrangThaiDonHang = dto.TrangThai;
            
            // Nếu có nhập mã vận đơn thì cập nhật
            if (!string.IsNullOrEmpty(dto.MaVanDon)) 
            {
                order.MaVanDon = dto.MaVanDon;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật đơn hàng thành công!" });
        }
    }

    // Class phụ để hứng dữ liệu cập nhật từ giao diện gửi xuống
    public class UpdateOrderDto
    {
        public string TrangThai { get; set; } = null!;
        public string? MaVanDon { get; set; }
    }
}