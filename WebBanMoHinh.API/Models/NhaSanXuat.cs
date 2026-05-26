using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class NhaSanXuat
{
    public int MaNsx { get; set; }

    public string TenNsx { get; set; } = null!;

    public string? QuocGia { get; set; }

    public virtual ICollection<SanPham> SanPham { get; set; } = new List<SanPham>();
}
