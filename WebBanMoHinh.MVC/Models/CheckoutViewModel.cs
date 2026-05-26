namespace WebBanMoHinh.MVC.Models
{
    public class CheckoutViewModel
    {
        public string? TenDangNhap { get; set; } = null!;
        public string DiaChiGiaoHang { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string PhuongThucThanhToan { get; set; } = "Thanh toán khi nhận hàng";
    }
}