using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;
using System.Net;
using System.Net.Mail;

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
            var checkExist = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == taiKhoan.TenDangNhap);
            if (checkExist)
            {
                return BadRequest(new { message = "Tên đăng nhập này đã tồn tại!" });
            }

            // Băm mật khẩu bằng BCrypt trước khi lưu
            taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(taiKhoan.MatKhau);
            
            taiKhoan.VaiTro = "Customer"; 
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
            // Tìm user chỉ bằng Username
            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == request.Username);

            // Xác thực mật khẩu bằng hàm Verify của BCrypt
            if (taiKhoan == null || !BCrypt.Net.BCrypt.Verify(request.Password, taiKhoan.MatKhau))
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
            }

            if (taiKhoan.TrangThai == false)
            {
                return BadRequest(new { message = "Tài khoản của bạn đã bị khóa!" });
            }

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

            taiKhoan.MatKhau = "********";
            return Ok(taiKhoan);
        }

        // 4. PUT: api/TaiKhoans/CapNhatProfile (Cập nhật thông tin & Đổi mật khẩu)
        [HttpPut("CapNhatProfile")]
        public async Task<IActionResult> CapNhatProfile([FromBody] UpdateProfileRequest request)
        {
            var taiKhoan = await _context.TaiKhoan.FindAsync(request.TenDangNhap);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy hồ sơ Pilot!" });

            taiKhoan.HoTen = request.HoTen ?? taiKhoan.HoTen;
            taiKhoan.Email = request.Email ?? taiKhoan.Email;
            taiKhoan.SoDienThoai = request.SoDienThoai ?? taiKhoan.SoDienThoai;
            taiKhoan.Avatar = request.Avatar ?? taiKhoan.Avatar;

            if (!string.IsNullOrEmpty(request.MatKhauCu) && !string.IsNullOrEmpty(request.MatKhauMoi))
            {
                // Kiểm tra mật khẩu cũ bằng BCrypt
                if (!BCrypt.Net.BCrypt.Verify(request.MatKhauCu, taiKhoan.MatKhau))
                {
                    return BadRequest(new { message = "Mật khẩu hiện tại không chính xác!" });
                }
                
                // Băm mật khẩu mới trước khi lưu
                taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhauMoi);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật hồ sơ Pilot thành công!" });
        }

        // ================= LUỒNG ĐĂNG NHẬP GOOGLE =================
        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email)) 
                return BadRequest(new { message = "Dữ liệu email trống!" });

            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == request.Email);

            if (taiKhoan == null)
            {
                string tempUsername = request.Email.Split('@')[0];

                var checkUsername = await _context.TaiKhoan.AnyAsync(t => t.TenDangNhap == tempUsername);
                if (checkUsername)
                {
                    tempUsername = tempUsername + new Random().Next(100, 999);
                }

                taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tempUsername,
                    // Băm mật khẩu ngẫu nhiên
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString().Substring(0, 8)),
                    HoTen = request.HoTen,
                    Email = request.Email,
                    VaiTro = "Customer", 
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };

                _context.TaiKhoan.Add(taiKhoan);
                await _context.SaveChangesAsync();
            }

            if (taiKhoan.TrangThai == false)
            {
                return BadRequest(new { message = "Tài khoản liên kết với Google này đã bị khóa!" });
            }

            return Ok(new
            {
                message = "Đăng nhập Google thành công!",
                username = taiKhoan.TenDangNhap,
                hoTen = taiKhoan.HoTen,
                vaiTro = taiKhoan.VaiTro,
                dataCart = taiKhoan.DiaChi,
                avatar = taiKhoan.Avatar 
            });
        }

        // ================= THÊM MỚI LUỒNG QUÊN MẬT KHẨU =================
        [HttpPost("QuenMatKhau")]
        public async Task<IActionResult> QuenMatKhau([FromBody] ForgotPasswordRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Vui lòng cung cấp Email." });
            }

            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == request.Email);
            if (taiKhoan == null)
            {
                return NotFound(new { message = "Email này chưa được liên kết với bất kỳ tài khoản Pilot nào!" });
            }

            // Tạo mật khẩu thô để gửi mail
            string newPassword = Guid.NewGuid().ToString().Substring(0, 8);

            // Băm mật khẩu trước khi cập nhật vào DB
            taiKhoan.MatKhau = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Gửi mật khẩu thô qua email
            bool sendMailSuccess = await SendEmailAsync(taiKhoan.Email!, taiKhoan.HoTen ?? "Pilot", newPassword);

            if (sendMailSuccess)
            {
                return Ok(new { message = "Đã gửi mật mã khôi phục vào email." });
            }
            else
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi gửi Email, vui lòng thử lại sau." });
            }
        }

        // ================= HÀM HỖ TRỢ GỬI EMAIL (SMTP) =================
        private async Task<bool> SendEmailAsync(string toEmail, string userName, string newPassword)
        {
            try
            {
                string fromEmail = "vonam06062005@gmail.com"; 
                string appPassword = "erdm ooho qjls ewhn"; 

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(fromEmail, "Tổng Kho KingGunpla");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = "[KingGunpla] Khôi phục mật mã đăng nhập Pilot";
                mailMessage.IsBodyHtml = true;
                
                mailMessage.Body = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #0f172a; color: #f8fafc;'>
                        <div style='max-width: 600px; margin: auto; background: #1e293b; padding: 30px; border-radius: 15px; border: 1px solid #00c6ff;'>
                            <h2 style='color: #00c6ff; text-align: center; text-transform: uppercase;'>KingGunpla Security</h2>
                            <p>Xin chào <strong>{userName}</strong>,</p>
                            <p>Hệ thống nhận được yêu cầu khôi phục mật khẩu cho tài khoản liên kết với email này.</p>
                            <p>Mật mã khôi phục (Mật khẩu mới) của bạn là:</p>
                            <div style='text-align: center; margin: 20px 0;'>
                                <span style='display: inline-block; padding: 15px 30px; background-color: #00c6ff; color: #fff; font-size: 24px; font-weight: bold; border-radius: 8px; letter-spacing: 2px;'>
                                    {newPassword}
                                </span>
                            </div>
                            <p>Vui lòng sử dụng mật mã này để đăng nhập lại hệ thống và tiến hành <strong>đổi mật khẩu ngay</strong> để đảm bảo an toàn.</p>
                            <hr style='border: none; border-top: 1px solid rgba(255,255,255,0.1); margin: 20px 0;' />
                            <p style='font-size: 12px; color: #8a93a2; text-align: center;'>Đây là email tự động từ hệ thống, vui lòng không trả lời thư này.</p>
                        </div>
                    </div>";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(fromEmail, appPassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mailMessage);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
    
    // ================= CÁC CLASS (DTO) NHẬN DỮ LIỆU TỪ MVC =================
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

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = null!;
    }
}