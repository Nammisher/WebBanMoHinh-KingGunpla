namespace WebBanMoHinh.API.Models
{
    public class CheckoutRequest
    {
        public string TenDangNhap { get; set; } = null!;
        public string DiaChiGiaoHang { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string PhuongThucThanhToan { get; set; } = "Thanh toán khi nhận hàng";
        public decimal TongTien { get; set; } // THÊM DÒNG NÀY ĐỂ HỨNG TỔNG TIỀN TỪ MVC
        public decimal PhiVanChuyen { get; set; } // THÊM DÒNG NÀY ĐỂ HỨNG PHÍ VẬN CHUYỂN TỪ MVC   
        public List<CartItemDTO> Items { get; set; } = new();
    }

    public class CartItemDTO
    {
        public int MaSp { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaBan { get; set; } // Đổi từ DonGia sang GiaBan cho khớp với MVC
    }
}