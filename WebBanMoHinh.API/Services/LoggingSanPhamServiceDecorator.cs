using WebBanMoHinh.API.Models;

namespace WebBanMoHinh.API.Services
{
    // Lớp Decorator: Bọc lại class cũ và bổ sung tính năng in Log theo dõi hệ thống
    public class LoggingSanPhamServiceDecorator : ISanPhamService
    {
        private readonly ISanPhamService _innerService;
        private readonly ILogger<LoggingSanPhamServiceDecorator> _logger;

        public LoggingSanPhamServiceDecorator(ISanPhamService innerService, ILogger<LoggingSanPhamServiceDecorator> logger)
        {
            _innerService = innerService;
            _logger = logger;
        }

        public async Task<IEnumerable<SanPham>> GetAllActiveProductsAsync()
        {
            _logger.LogInformation(">>> [HỆ THỐNG] Bắt đầu gọi API lấy danh sách mô hình từ Database...");
            
            var startTime = DateTime.Now;
            var products = await _innerService.GetAllActiveProductsAsync(); // Gọi class gốc
            var duration = (DateTime.Now - startTime).TotalMilliseconds;

            _logger.LogInformation($">>> [HỆ THỐNG] Lấy dữ liệu thành công! Tổng số: {products.Count()} món. Thời gian tải: {duration}ms.");
            return products;
        }
    }
}