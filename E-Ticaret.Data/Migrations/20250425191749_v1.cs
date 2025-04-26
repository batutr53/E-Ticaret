using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "Logo", "Name" },
                values: new object[] { 1, new DateTime(2024, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Detay Çiçek", true, "", "Detay Çiçek" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ClassImage", "CreatedDate", "Description", "Image", "IsActive", "IsTopMenu", "Name", "OrderNo", "ParentId" },
                values: new object[,]
                {
                    { 1, "fa fa-home", new DateTime(2024, 4, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Kategoriler", "", true, true, "Kategoriler", 1, 0 },
                    { 2, "fa fa-gift", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Özel günler ve kutlamalar için hediyelik ürünler", "", true, true, "Hediyelik", 2, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
