using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class LichSuDauGia
{
    public int MaLuotBidding { get; set; }

    public int MaPhien { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public decimal SoTienDau { get; set; }

    public DateTime? ThoiGianDau { get; set; }

    public virtual PhienDauGia MaPhienNavigation { get; set; } = null!;

    public virtual TaiKhoan TenDangNhapNavigation { get; set; } = null!;
}
