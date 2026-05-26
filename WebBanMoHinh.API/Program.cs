using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Đảm bảo có cái này để nhận diện ILogger
using WebBanMoHinh.API.Services;     // Kích hoạt thư viện Services để gọi tên ngắn gọn

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối SQL Server
builder.Services.AddDbContext<WebBanMoHinh.API.Models.WebBanMoHinhContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. BẮT BUỘC: Đăng ký dịch vụ Controller và xử lý vòng lặp JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 3. Đăng ký dịch vụ Swagger & HttpClient
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// --- ĐĂNG KÝ DESIGN PATTERNS (Đã được thu gọn tên nhờ lệnh using ở đầu file) ---

// [Factory Method & Facade Pattern]
builder.Services.AddSingleton<PaymentFactory>();
builder.Services.AddScoped<IOrderFacade, OrderFacade>();

// [Decorator Pattern]
builder.Services.AddScoped<SanPhamService>();
builder.Services.AddScoped<ISanPhamService>(provider => 
{
    var coreService = provider.GetRequiredService<SanPhamService>();
    var logger = provider.GetRequiredService<ILogger<LoggingSanPhamServiceDecorator>>();
    
    // Trả về đối tượng Core đã được bao bọc (Decorate) bởi lớp ghi Log
    return new LoggingSanPhamServiceDecorator(coreService, logger);
});


var app = builder.Build();

// Cấu hình Pipeline xử lý HTTP requests
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5. BẮT BUỘC: Bản đồ hóa các tuyến đường API từ các Controller
app.MapControllers();

// Giữ lại API thời tiết mặc định để test song song
var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
}).WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}