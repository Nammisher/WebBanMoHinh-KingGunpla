using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaiKhoansController : ControllerBase
    {
        private readonly WebBanMoHinhContext _context;

        public TaiKhoansController(WebBanMoHinhContext context)
        {
            _context = context;
        }

        // 1. POST: api/TaiKhoans/DangKy (Đăng ký tài khoản)
        [HttpPost("DangKy")]
        public async Task<IActionResult> DangKy(TaiKhoan taiKhoan)
        {
            // Kiểm tra xem tên đăng nhập đã tồn tại chưa
            var checkExist = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap);
            if (checkExist)
            {
                return BadRequest(new { message = "Tên đăng nhập này đã tồn tại!" });
            }

            taiKhoan.VaiTro = "Customer"; // Mặc định tự đăng ký là Khách hàng
            taiKhoan.TrangThai = true;
            taiKhoan.NgayTao = DateTime.Now;

            _context.TaiKhoan.Add(taiKhoan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        // 2. POST: api/TaiKhoans/DangNhap (Đăng nhập đơn giản)
        [HttpPost("DangNhap")]
        public async Task<IActionResult> DangNhap([FromBody] LoginRequest request)
        {
            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == request.Username && t.MatKhau == request.Password);

            if (taiKhoan == null)
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            if (taiKhoan.TrangThai == false)
            {
                return BadRequest(new { message = "Tài khoản của bạn đã bị khóa!" });
            }

            // Trả về thông tin cơ bản để Frontend lưu Session/Cookie
            return Ok(new
            {
                message = "Đăng nhập thành công!",
                username = taiKhoan.TenDangNhap,
                hoTen = taiKhoan.HoTen,
                vaiTro = taiKhoan.VaiTro,
                avatar = taiKhoan.Avatar
            });
        }

        // 3. GET: api/TaiKhoans/Profile/demo12 (Lấy thông tin cá nhân)
        [HttpGet("Profile/{username}")]
        public async Task<ActionResult<TaiKhoan>> GetProfile(string username)
        {
            var taiKhoan = await _context.TaiKhoan.FindAsync(username);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy người dùng!" });

            // Giấu mật khẩu đi trước khi trả về Frontend để bảo mật
            taiKhoan.MatKhau = "********";
            return Ok(taiKhoan);
        }

        // 4. PUT: api/TaiKhoans/CapNhatProfile (Cập nhật thông tin & Đổi mật khẩu)
        [HttpPut("CapNhatProfile")]
        public async Task<IActionResult> CapNhatProfile([FromBody] UpdateProfileRequest request)
        {
            // Tìm tài khoản trong Database
            var taiKhoan = await _context.TaiKhoan.FindAsync(request.TenDangNhap);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy hồ sơ Pilot!" });

            // 1. Cập nhật thông tin cơ bản (Nếu người dùng không nhập thì giữ nguyên dữ liệu cũ)
            taiKhoan.HoTen = request.HoTen ?? taiKhoan.HoTen;
            taiKhoan.Email = request.Email ?? taiKhoan.Email;
            taiKhoan.SoDienThoai = request.SoDienThoai ?? taiKhoan.SoDienThoai;
            taiKhoan.Avatar = request.Avatar ?? taiKhoan.Avatar;
            // 2. Xử lý logic Đổi mật khẩu (Chỉ chạy nếu user có nhập mật khẩu cũ & mới)
            if (!string.IsNullOrEmpty(request.MatKhauCu) && !string.IsNullOrEmpty(request.MatKhauMoi))
            {
                // Kiểm tra xem mật khẩu cũ nhập vào có khớp với DB không
                if (taiKhoan.MatKhau != request.MatKhauCu)
                {
                    return BadRequest(new { message = "Mật khẩu hiện tại không chính xác!" });
                }
                
                // Hợp lệ thì đổi sang mật khẩu mới
                taiKhoan.MatKhau = request.MatKhauMoi;
            }

            // Lưu thay đổi xuống Database
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ Pilot thành công!" });
        }

        // ================= THÊM MỚI LUỒNG ĐĂNG NHẬP GOOGLE =================
        // API: POST api/TaiKhoans/GoogleLogin
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email)) 
                return BadRequest(new { message = "Dữ liệu email trống!" });

            // 1. Quét DB xem Email này đã từng tồn tại trong hệ thống chưa
            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == request.Email);

            // 2. Nếu chưa từng có tài khoản, hệ thống tự tạo mới luôn (Auto-Register)
            if (taiKhoan == null)
            {
                // Cắt chuỗi Email lấy phần trước chữ @ làm Tên đăng nhập tạm thời
                string tempUsername = request.Email.Split('@')[0];

                // Phòng trường hợp tên đăng nhập này bị trùng, ta gắn thêm số ngẫu nhiên
                var checkUsername = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == tempUsername);
                if (checkUsername)
                {
                    tempUsername = tempUsername + new Random().Next(100, 999);
                }

                taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tempUsername,
                    MatKhau = Guid.NewGuid().ToString().Substring(0, 8), // Mật khẩu ngẫu nhiên bảo mật
                    HoTen = request.HoTen,
                    Email = request.Email,
                    VaiTro = "Customer", // Mặc định là Khách hàng
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };

                _context.TaiKhoan.Add(taiKhoan);
                await _context.SaveChangesAsync();
            }

            // 3. Nếu tài khoản đang bị khóa
            if (taiKhoan.TrangThai == false)
            {
                return BadRequest(new { message = "Tài khoản liên kết với Google này đã bị khóa!" });
            }

            // 4. Trả về cấu trúc giống hệt API Đăng nhập thông thường để MVC tái tạo lại Giỏ hàng "GioHang"
            return Ok(new
            {
                message = "Đăng nhập Google thành công!",
                username = taiKhoan.TenDangNhap,
                hoTen = taiKhoan.HoTen,
                vaiTro = taiKhoan.VaiTro,
                dataCart = taiKhoan.DiaChi,
                avatar = taiKhoan.Avatar // Trả kèm chuỗi JSON giỏ hàng cũ để khôi phục
            });
        }
    }
    
    public class UpdateProfileRequest
    {
        public string TenDangNhap { get; set; } = null!;
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? MatKhauCu { get; set; }
        public string? MatKhauMoi { get; set; }
        public string? Avatar { get; set; }
    }

    // Class phụ hỗ trợ hứng dữ liệu đăng nhập gọn gàng
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class GoogleLoginRequest
    {
        public string Email { get; set; } = null!;
        public string HoTen { get; set; } = null!;
    }
}