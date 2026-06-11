namespace WebBanMoHinh.API.Models
{
    public partial class GioHang
    {
        public string TenDangNhap { get; set; } = null!;
        public string? CartJson { get; set; }
        public virtual TaiKhoan TenDangNhapNavigation { get; set; } = null!;
    }
}