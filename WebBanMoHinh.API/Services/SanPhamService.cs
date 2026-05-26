using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Services
{
    // Lớp chịu trách nhiệm chính: Chỉ lấy dữ liệu gốc từ DB
    public class SanPhamService : ISanPhamService
    {
        private readonly WebBanMoHinhContext _context;
        public SanPhamService(WebBanMoHinhContext context) => _context = context;

        public async Task<IEnumerable<SanPham>> GetAllActiveProductsAsync()
        {
            return await _context.SanPham.Where(s => s.DaXoa == false || s.DaXoa == null).ToListAsync();
        }
    }
}