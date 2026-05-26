// File: OrderHistoryViewModel.cs
namespace WebBanMoHinh.MVC.Models
{
    public class OrderHistoryViewModel
    {
        public int MaDonHang { get; set; }
        public DateTime? NgayDat { get; set; }
        public decimal? TongTien { get; set; }
        public string? PhuongThucThanhToan { get; set; }
        public string? TrangThaiDonHang { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        
        // Danh sách các món trong đơn này
        public List<OrderDetailViewModel> ChiTietDonHang { get; set; } = new();
    }

    public class OrderDetailViewModel
    {
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        
        // Thông tin sản phẩm đi kèm (được API gộp vào qua ThenInclude)
        public SanPhamViewModel? MaSpNavigation { get; set; } 
    }
}