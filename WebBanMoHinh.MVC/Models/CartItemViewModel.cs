namespace WebBanMoHinh.MVC.Models
{
    public class CartItemViewModel
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; } = null!;
        public string? HinhAnhChinh { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        
        // Tự động tính thành tiền của món đồ này
        public decimal ThanhTien => GiaBan * SoLuong;
    }
}