using System.ComponentModel.DataAnnotations;

namespace WebBanMoHinh.MVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tài khoản hoặc Email")]
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = null!;

        // Giữ lại đường dẫn trang trước đó để sau khi đăng nhập thành công thì nhảy về
        public string? ReturnUrl { get; set; }
    }
}