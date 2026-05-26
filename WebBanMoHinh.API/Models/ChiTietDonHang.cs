using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class ChiTietDonHang
{
    public int MaChiTiet { get; set; }

    public int? MaDonHang { get; set; }

    public int? MaSp { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public virtual DonHang? MaDonHangNavigation { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
