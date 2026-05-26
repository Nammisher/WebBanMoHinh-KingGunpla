namespace WebBanMoHinh.API.Models
{
    public class CheckoutRequest
    {
        public string TenDangNhap { get; set; } = null!;
        public string DiaChiGiaoHang { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string PhuongThucThanhToan { get; set; } = "Thanh toán khi nhận hàng";
        public List<CartItemDTO> Items { get; set; } = new();
    }

    public class CartItemDTO
    {
        public int MaSp { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaBan { get; set; } // Đổi từ DonGia sang GiaBan cho khớp với MVC
    }
}