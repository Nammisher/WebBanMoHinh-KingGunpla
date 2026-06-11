using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebBanMoHinh.API.Models;

public partial class WebBanMoHinhContext : DbContext
{
    public WebBanMoHinhContext()
    {
    }

    public WebBanMoHinhContext(DbContextOptions<WebBanMoHinhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BinhLuanDanhGia> BinhLuanDanhGia { get; set; }

    public virtual DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }

    public virtual DbSet<DonHang> DonHang { get; set; }

    public virtual DbSet<DongSanPham> DongSanPham { get; set; }

    public virtual DbSet<HinhAnhSanPham> HinhAnhSanPham { get; set; }

    public virtual DbSet<LichSuDauGia> LichSuDauGia { get; set; }

    public virtual DbSet<LoaiSanPham> LoaiSanPham { get; set; }

    public virtual DbSet<NhaSanXuat> NhaSanXuat { get; set; }

    public virtual DbSet<PhienDauGia> PhienDauGia { get; set; }

    public virtual DbSet<SanPham> SanPham { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoan { get; set; }
    public virtual DbSet<GioHang> GioHang { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Để trống hoàn toàn
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BinhLuanDanhGia>(entity =>
        {
            entity.HasKey(e => e.MaBinhLuan).HasName("PK__BinhLuan__87CB66A0BE5B805F");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.NgayBinhLuan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.BinhLuanDanhGia)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_BL_SP");

            entity.HasOne(d => d.TenDangNhapNavigation).WithMany(p => p.BinhLuanDanhGia)
                .HasForeignKey(d => d.TenDangNhap)
                .HasConstraintName("FK_BL_TK");
        });

        modelBuilder.Entity<ChiTietDonHang>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietD__CDF0A1143BEBBEC8");

            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaSp).HasColumnName("MaSP");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.ChiTietDonHang)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK_CTDH_DonHang");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.ChiTietDonHang)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_CTDH_SP");
        });

        modelBuilder.Entity<DonHang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang).HasName("PK__DonHang__129584ADCC7F0EE2");

            entity.Property(e => e.DiaChiGiaoHang).HasMaxLength(500);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.NgayDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThaiDonHang)
                .HasMaxLength(50)
                .HasDefaultValue("Chờ xác nhận");

            entity.HasOne(d => d.TenDangNhapNavigation).WithMany(p => p.DonHang)
                .HasForeignKey(d => d.TenDangNhap)
                .HasConstraintName("FK_DonHang_TK");
        });

        modelBuilder.Entity<DongSanPham>(entity =>
        {
            entity.HasKey(e => e.MaDong).HasName("PK__DongSanP__2DC5878944E16099");

            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenDong).HasMaxLength(100);
        });

        modelBuilder.Entity<HinhAnhSanPham>(entity =>
        {
            entity.HasKey(e => e.MaAnh).HasName("PK__HinhAnhS__356240DF9C92BAE7");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.UrlAnh).HasMaxLength(500);

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.HinhAnhSanPham)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_Anh_SP");
        });

        modelBuilder.Entity<LichSuDauGia>(entity =>
        {
            entity.HasKey(e => e.MaLuotBidding).HasName("PK__LichSuDa__DC59C3250C9DC9C9");

            entity.Property(e => e.SoTienDau).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ThoiGianDau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaPhienNavigation).WithMany(p => p.LichSuDauGia)
                .HasForeignKey(d => d.MaPhien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LSDG_Phien");

            entity.HasOne(d => d.TenDangNhapNavigation).WithMany(p => p.LichSuDauGia)
                .HasForeignKey(d => d.TenDangNhap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LSDG_TaiKhoan");
        });

        modelBuilder.Entity<LoaiSanPham>(entity =>
        {
            entity.HasKey(e => e.MaLoai).HasName("PK__LoaiSanP__730A5759F0AC8C39");

            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<NhaSanXuat>(entity =>
        {
            entity.HasKey(e => e.MaNsx).HasName("PK__NhaSanXu__3A1BDBD23F33979E");

            entity.Property(e => e.MaNsx).HasColumnName("MaNSX");
            entity.Property(e => e.QuocGia).HasMaxLength(100);
            entity.Property(e => e.TenNsx)
                .HasMaxLength(200)
                .HasColumnName("TenNSX");
        });

        modelBuilder.Entity<PhienDauGia>(entity =>
        {
            entity.HasKey(e => e.MaPhien).HasName("PK__PhienDau__2660BFEF9EA830D6");

            entity.Property(e => e.BuocGiaMin).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaHienTai).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaKhoiDiem).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaMuaNgay).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("SapDienRa");
            entity.Property(e => e.VersRow)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.PhienDauGia)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PDG_SanPham");
        });

        modelBuilder.Entity<SanPham>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__SanPham__2725081C6E032B48");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.ChatLieu).HasMaxLength(100);
            entity.Property(e => e.DaXoa).HasDefaultValue(false);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaGiam)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HinhAnhChinh).HasMaxLength(500);
            entity.Property(e => e.IsHot).HasDefaultValue(false);
            entity.Property(e => e.KichThuoc)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LoaiHinhHienThi)
                .HasMaxLength(50)
                .HasDefaultValue("BanThang");
            entity.Property(e => e.MaNsx).HasColumnName("MaNSX");
            entity.Property(e => e.NgayNhap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoLuongTon).HasDefaultValue(0);
            entity.Property(e => e.TenSp)
                .HasMaxLength(500)
                .HasColumnName("TenSP");
            entity.Property(e => e.TiLe)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaDongNavigation).WithMany(p => p.SanPham)
                .HasForeignKey(d => d.MaDong)
                .HasConstraintName("FK_SP_DongSanPham");

            entity.HasOne(d => d.MaLoaiNavigation).WithMany(p => p.SanPham)
                .HasForeignKey(d => d.MaLoai)
                .HasConstraintName("FK_SP_Loai");

            entity.HasOne(d => d.MaNsxNavigation).WithMany(p => p.SanPham)
                .HasForeignKey(d => d.MaNsx)
                .HasConstraintName("FK_SP_NSX");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TenDangNhap).HasName("PK__TaiKhoan__55F68FC181E43542");

            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(200);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro)
                .HasMaxLength(20)
                .HasDefaultValue("Customer");
        });
        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.TenDangNhap);
            
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.TenDangNhapNavigation)
                .WithOne()
                .HasForeignKey<GioHang>(d => d.TenDangNhap)
                .HasConstraintName("FK_GioHang_TaiKhoan");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
