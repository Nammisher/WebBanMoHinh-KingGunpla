using System.Text.Json.Serialization; 

namespace WebBanMoHinh.MVC.Models
{
    public class SanPhamViewModel
    {
        public int MaSp { get; set; }
        public string TenSp { get; set; } = null!;
        public decimal? GiaBan { get; set; }
        public decimal? GiaGiam { get; set; }
        public int? SoLuongTon { get; set; }
        public string? HinhAnhChinh { get; set; }
        public string? MoTa { get; set; }
        public string? TiLe { get; set; }
        public string? KichThuoc { get; set; }
        public string? ChatLieu { get; set; }

        public bool? IsHot { get; set; }
        public int? MaDong { get; set; }
        public int? MaNsx { get; set; }

        // Vẫn gọi được NhaSanXuatViewModel bình thường vì cùng Namespace
        public NhaSanXuatViewModel? MaNsxNavigation { get; set; }

        [JsonPropertyName("binhLuanDanhGia")]
        public ICollection<BinhLuanDanhGiaViewModel>? BinhLuanDanhGias { get; set; }
    }
}