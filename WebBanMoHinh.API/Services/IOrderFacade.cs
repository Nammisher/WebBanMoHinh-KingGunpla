using Microsoft.EntityFrameworkCore;
using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Services
{
    public interface IOrderFacade
    {
        Task<(bool Success, string Message, int OrderId)> PlaceOrderAsync(DonHang donHang);
    }
}