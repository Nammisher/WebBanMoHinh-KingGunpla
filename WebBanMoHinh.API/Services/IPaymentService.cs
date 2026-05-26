using System.Threading.Tasks;

namespace WebBanMoHinh.API.Services
{
    // Giao diện chung cho mọi phương thức thanh toán
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(decimal amount);
    }

    // Xử lý thanh toán khi nhận hàng (COD)
    public class CodPaymentService : IPaymentService
    {
        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            await Task.Delay(100); // Giả lập xử lý nhanh
            return true; 
        }
    }

    // Xử lý thanh toán qua ví MoMo
    public class MoMoPaymentService : IPaymentService
    {
        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            await Task.Delay(500); // Giả lập gọi API sang MoMo
            // Ở đây sau này bạn sẽ viết logic gọi API MoMo thật
            return true; 
        }
    }

    // ========================================================
    // ĐÃ THÊM: Strategy xử lý thanh toán Chuyển khoản Online
    // ========================================================
    public class ChuyenKhoanOnlinePaymentService : IPaymentService
    {
        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {
            // Do phần tạo mã QR đã được xử lý bên MVC, API chỉ cần lưu Database thành công là được!
            await Task.Delay(100); 
            return true; 
        }
    }
}