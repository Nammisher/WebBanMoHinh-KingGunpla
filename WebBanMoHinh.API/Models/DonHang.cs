using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class DonHang
{
    public int MaDonHang { get; set; }

    public string? TenDangNhap { get; set; }

    public string? SoDienThoai { get; set; }
    
    public DateTime? NgayDat { get; set; }

    public decimal? TongTien { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public string? TrangThaiDonHang { get; set; }

    public string? DiaChiGiaoHang { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; } = new List<ChiTietDonHang>();

    public virtual TaiKhoan? TenDangNhapNavigation { get; set; }
}
