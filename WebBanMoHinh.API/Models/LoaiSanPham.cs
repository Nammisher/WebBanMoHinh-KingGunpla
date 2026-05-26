using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class LoaiSanPham
{
    public int MaLoai { get; set; }

    public string TenLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<SanPham> SanPham { get; set; } = new List<SanPham>();
}
