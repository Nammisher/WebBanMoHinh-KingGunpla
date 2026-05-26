using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Services
{
    public interface ISanPhamService
    {
        Task<IEnumerable<SanPham>> GetAllActiveProductsAsync();
    }
}