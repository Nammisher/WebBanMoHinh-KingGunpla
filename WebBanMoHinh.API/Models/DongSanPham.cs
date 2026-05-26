using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class DongSanPham
{
    public int MaDong { get; set; }

    public string TenDong { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<SanPham> SanPham { get; set; } = new List<SanPham>();
}
