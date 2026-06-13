using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBanMoHinh.API.Models;

public partial class SanPham
{
    public int MaSp { get; set; }
    public string TenSp { get; set; } = null!;
    public decimal GiaBan { get; set; }
    public decimal? GiaGiam { get; set; }
    public int? SoLuongTon { get; set; }
    public string? HinhAnhChinh { get; set; }
    public string? MoTa { get; set; }
    public string? TiLe { get; set; }
    public string? KichThuoc { get; set; }
    public string? ChatLieu { get; set; }
    public int? MaLoai { get; set; }
    
    // Đã đổi thành MaNsx (chữ thường sx)
    public int? MaNsx { get; set; }

    public DateTime? NgayNhap { get; set; }
    public bool? DaXoa { get; set; }
    public int? MaDong { get; set; }
    public bool? IsHot { get; set; }
    public string? LoaiHinhHienThi { get; set; }

    [Timestamp] 
    public required byte[] Version { get; set; }
    
    public virtual ICollection<BinhLuanDanhGia> BinhLuanDanhGia { get; set; } = new List<BinhLuanDanhGia>();
    public virtual ICollection<ChiTietDonHang> ChiTietDonHang { get; set; } = new List<ChiTietDonHang>();
    public virtual ICollection<HinhAnhSanPham> HinhAnhSanPham { get; set; } = new List<HinhAnhSanPham>();
    public virtual DongSanPham? MaDongNavigation { get; set; }
    public virtual LoaiSanPham? MaLoaiNavigation { get; set; }
    public virtual NhaSanXuat? MaNsxNavigation { get; set; }
    public virtual ICollection<PhienDauGia> PhienDauGia { get; set; } = new List<PhienDauGia>();
}