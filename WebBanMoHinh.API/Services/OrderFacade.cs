using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Services
{
    public class OrderFacade : IOrderFacade
    {
        private readonly WebBanMoHinhContext _context;
        private readonly PaymentFactory _paymentFactory;

        public OrderFacade(WebBanMoHinhContext context, PaymentFactory paymentFactory)
        {
            _context = context;
            _paymentFactory = paymentFactory;
        }

        public async Task<(bool Success, string Message, int OrderId)> PlaceOrderAsync(DonHang donHang)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lưu thông tin đơn hàng gốc
                donHang.NgayDat = DateTime.Now;
                donHang.TrangThaiDonHang = "Chờ xác nhận";
                _context.DonHang.Add(donHang);
                await _context.SaveChangesAsync();

                // 2. Duyệt chi tiết đơn hàng để trừ kho sản phẩm
                foreach (var ct in donHang.ChiTietDonHang)
                {
                    var sanPham = await _context.SanPham.FindAsync(ct.MaSp);
                    if (sanPham == null) return (false, $"Không tìm thấy sản phẩm ID: {ct.MaSp}", 0);
                    if (sanPham.SoLuongTon < ct.SoLuong) return (false, $"Sản phẩm {sanPham.TenSp} không đủ tồn kho!", 0);

                    sanPham.SoLuongTon -= ct.SoLuong; // Trừ kho
                }

                // 3. Sử dụng Factory Method để xử lý thanh toán tự động
                if (string.IsNullOrWhiteSpace(donHang.PhuongThucThanhToan))
                {
                    await transaction.RollbackAsync();
                    return (false, "Phương thức thanh toán không hợp lệ.", 0);
                }

                var paymentProcessor = _paymentFactory.CreatePaymentMethod(donHang.PhuongThucThanhToan!);
                bool paymentSuccess = await paymentProcessor.ProcessPaymentAsync(donHang.TongTien ?? 0);

                if (!paymentSuccess)
                {
                    await transaction.RollbackAsync();
                    return (false, "Thanh toán thất bại!", 0);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Đặt hàng thành công!", donHang.MaDonHang);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi hệ thống: {ex.Message}", 0);
            }
        }
    }
}