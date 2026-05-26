using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class PhienDauGia
{
    public int MaPhien { get; set; }

    public int MaSp { get; set; }

    public decimal GiaKhoiDiem { get; set; }

    public decimal? GiaHienTai { get; set; }

    public decimal? GiaMuaNgay { get; set; }

    public decimal BuocGiaMin { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public string? TrangThai { get; set; }

    public byte[] VersRow { get; set; } = null!;

    public virtual ICollection<LichSuDauGia> LichSuDauGia { get; set; } = new List<LichSuDauGia>();

    public virtual SanPham MaSpNavigation { get; set; } = null!;
}
