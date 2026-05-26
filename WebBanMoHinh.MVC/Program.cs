using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// PHẦN 1: ĐĂNG KÝ CÁC DỊCH VỤ (SERVICES) - PHẢI NẰM TRÊN BUILD()
// =========================================================

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Cấu hình Session (Giỏ hàng)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ĐĂNG KÝ AUTHENTICATION VÀ GOOGLE LOGIC Ở ĐÂY
// ĐỌC MÃ BẢO MẬT TỪ APPSETTINGS.JSON RA BIẾN CỤC BỘ (TRÁNH LỖI SCOPE BUILDER)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";

// ĐĂNG KÝ AUTHENTICATION VÀ GOOGLE LOGIC Ở ĐÂY
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options => 
{
    options.LoginPath = "/Account/Login"; // Đá người dùng về trang này nếu chưa đăng nhập
})
.AddGoogle(options =>
{
    // Sử dụng 2 biến cục bộ đã lấy ở trên
    options.ClientId = googleClientId;
    options.ClientSecret = googleClientSecret;
});


// =========================================================
// PHẦN 2: CHẠY BUILD VÀ CẤU HÌNH MIDDLEWARE PIPELINE
// =========================================================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// THỨ TỰ BẮT BUỘC KHÔNG ĐƯỢC ĐẢO LỘN
app.UseRouting();         
app.UseSession();         // Kích hoạt Session

app.UseAuthentication();  // BẮT BUỘC PHẢI NẰM Ở ĐÂY (TRÊN UseAuthorization)
app.UseAuthorization();   

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();