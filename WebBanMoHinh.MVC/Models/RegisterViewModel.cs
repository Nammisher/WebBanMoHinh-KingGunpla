using System.ComponentModel.DataAnnotations;

namespace WebBanMoHinh.MVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản mong muốn")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Tài khoản phải từ 4 đến 20 ký tự")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tài khoản không được chứa ký tự đặc biệt hoặc khoảng trắng")]
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string HoTen { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})\b$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
        public string SoDienThoai { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu kích hoạt")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng xác nhận lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không trùng khớp! Vui lòng gõ lại")]
        public string NhapLaiMatKhau { get; set; } = null!;
    }
}