using System;

namespace WebBanMoHinh.API.Services
{
    public class PaymentFactory
    {
        public IPaymentService CreatePaymentMethod(string method)
        {
            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException("Phương thức thanh toán bị trống!");

            // Chuyển hết thành chữ IN HOA để dễ bắt từ khóa
            string normalized = method.Trim().ToUpper();

            // Dùng .Contains() để "bắt từ khóa" thay vì khớp cứng 100%
            if (normalized == "COD" || normalized.Contains("NHẬN HÀNG"))
                return new CodPaymentService();

            if (normalized.Contains("MOMO"))
                return new MoMoPaymentService();

            // Chỉ cần chữ gửi xuống có chứa "CHUYỂN KHOẢN" hoặc "ONLINE" là lụm!
            if (normalized.Contains("CHUYỂN KHOẢN") || normalized.Contains("ONLINE") || normalized.Contains("CHUYEN KHOAN"))
                return new ChuyenKhoanOnlinePaymentService();

            // NẾU VẪN LỖI, IN THẲNG CÁI CHỮ ĐÓ RA MÀN HÌNH ĐỂ BẮT TẬN TAY!
            throw new ArgumentException($"Không hỗ trợ phương thức: '{method}'");
        }
    }
}