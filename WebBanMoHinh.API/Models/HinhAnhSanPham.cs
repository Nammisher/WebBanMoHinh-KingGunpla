using System;
using System.Collections.Generic;

namespace WebBanMoHinh.API.Models;

public partial class HinhAnhSanPham
{
    public int MaAnh { get; set; }

    public int? MaSp { get; set; }

    public string? UrlAnh { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
