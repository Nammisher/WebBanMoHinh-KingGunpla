using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class TaiKhoan
{
    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public string? VaiTro { get; set; }

    public DateTime? NgayTao { get; set; }

    public bool? TrangThai { get; set; }
    public string? Avatar { get; set; }

    public virtual ICollection<BinhLuanDanhGia> BinhLuanDanhGia { get; set; } = new List<BinhLuanDanhGia>();

    public virtual ICollection<DonHang> DonHang { get; set; } = new List<DonHang>();

    public virtual ICollection<LichSuDauGia> LichSuDauGia { get; set; } = new List<LichSuDauGia>();
}
