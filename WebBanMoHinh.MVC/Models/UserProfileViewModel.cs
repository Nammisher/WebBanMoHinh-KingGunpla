namespace WebBanMoHinh.MVC.Models
{
    public class UserProfileViewModel
    {
        public string TenDangNhap { get; set; } = null!;
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? VaiTro { get; set; }
        public DateTime? NgayTao { get; set; }

        // Các trường phục vụ cho tính năng đổi mật khẩu
        public string? MatKhauCu { get; set; }
        public string? MatKhauMoi { get; set; }
        public string? NhapLaiMatKhauMoi { get; set; }
        public string? Avatar { get; set; } // Dùng để hiển thị tên ảnh từ DB
        public IFormFile? AvatarUpload { get; set; } // Dùng để hứng file khách hàng upload
    }
}