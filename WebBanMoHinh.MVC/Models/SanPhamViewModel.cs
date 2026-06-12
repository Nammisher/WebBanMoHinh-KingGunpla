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

        // ==============================================================
        // THÊM CÁC THUỘC TÍNH NÀY ĐỂ ĐỒNG BỘ VỚI CÁC BỘ LỌC ĐỘNG Ở VIEW
        // ==============================================================
        public bool? IsHot { get; set; }
        public int? MaDong { get; set; }
        public int? MaNsx { get; set; }

        // Đối tượng điều hướng để lấy tên nhà sản xuất (Bandai, Motor Nuclear...)
        public NhaSanXuatViewModel? MaNsxNavigation { get; set; }

        // Danh sách các đánh giá của sản phẩm này
        public ICollection<BinhLuanDanhGiaViewModel>? BinhLuanDanhGias { get; set; }
    }
    // Lớp phụ dùng để hứng thông tin tên nhà sản xuất từ API Backend trả về
    public class NhaSanXuatViewModel
    {
        public int MaNsx { get; set; }
        public string? TenNsx { get; set; }
        public string? QuocGia { get; set; }
    }
}