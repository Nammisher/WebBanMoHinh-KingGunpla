using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class BinhLuanDanhGia
{
    public int MaBinhLuan { get; set; }

    public int? MaSp { get; set; }

    public string? TenDangNhap { get; set; }

    public string? NoiDung { get; set; }

    public int? SoSao { get; set; }

    public DateTime? NgayBinhLuan { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }

    public virtual TaiKhoan? TenDangNhapNavigation { get; set; }
}
