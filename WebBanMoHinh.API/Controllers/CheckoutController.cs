using Microsoft.AspNetCore.Mvc;
using WebBanMoHinh.API.Models;      
using WebBanMoHinh.API.Services;    
using System.Linq;   
using System;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")] 
    [ApiController]             
    public class CheckoutController : ControllerBase 
    {
        private readonly IOrderFacade _orderFacade; 

        public CheckoutController(IOrderFacade orderFacade)
        {
            _orderFacade = orderFacade;
        }

        [HttpPost("DatHang")]
        public async Task<IActionResult> DatHang([FromBody] CheckoutRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { message = "Đơn hàng của bạn không có sản phẩm nào!" });
            }

            try
            {
                var donHang = new DonHang
                {
                    TenDangNhap = request.TenDangNhap,
                    DiaChiGiaoHang = request.DiaChiGiaoHang,
                    SoDienThoai = request.SoDienThoai,
                    PhuongThucThanhToan = request.PhuongThucThanhToan,
                    NgayDat = DateTime.Now,
                    TrangThaiDonHang = "Chờ xác nhận",
                    TongTien = request.TongTien,
                    
                    // BỔ SUNG DÒNG NÀY: Hứng phí vận chuyển từ Request (MVC) gán vào DB
                    PhiVanChuyen = request.PhiVanChuyen,
                    
                    ChiTietDonHang = request.Items.Select(item => new ChiTietDonHang
                    {
                        MaSp = item.MaSp,
                        SoLuong = item.SoLuong,
                        DonGia = item.GiaBan
                    }).ToList()
                };

                // Đẩy DonHang (đã có PhiVanChuyen) vào Facade. 
                // Facade sẽ tự động xử lý Trừ Kho, Gọi Payment, và Lưu Database mượt mà!
                var result = await _orderFacade.PlaceOrderAsync(donHang);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = "Đặt hàng thành công qua Facade!", maDonHang = result.OrderId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi hệ thống khi xử lý Facade: " + ex.Message });
            }
        }
    }
}