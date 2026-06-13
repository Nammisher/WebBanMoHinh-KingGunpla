using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
//Sử dụng Entity Framework Core Code First và Migration để quản lý phiên bản cơ sở dữ liệu, 
//tự động tạo và cập nhật schema SQL Server.
namespace WebBanMoHinh.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionToSanPham : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ thêm cột Version vào bảng SanPham
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "SanPham",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa cột Version nếu cần rollback
            migrationBuilder.DropColumn(
                name: "Version",
                table: "SanPham");
        }
    }
}