namespace WebBanMoHinh.MVC.Models
{
    public class LoaiSanPhamViewModel 
    { 
        public int MaLoai { get; set; } 
        public string TenLoai { get; set; } = null!; 
        public string? MoTa { get; set; } 
    }

    public class DongSanPhamViewModel 
    { 
        public int MaDong { get; set; } 
        public string TenDong { get; set; } = null!; 
        public string? MoTa { get; set; } 
    }
    public class NhaSanXuatViewModel
    {
        public int MaNsx { get; set; }
        public string? TenNsx { get; set; }
        public string? QuocGia { get; set; }
    }
}