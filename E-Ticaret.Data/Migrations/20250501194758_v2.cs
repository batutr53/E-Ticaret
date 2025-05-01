using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ClassImage", "CreatedDate", "Description", "Image", "IsActive", "IsTopMenu", "Name", "OrderNo", "ParentId" },
                values: new object[,]
                {
                    { 3, "fa-solid fa-seedling", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Canlı Çiçekler", "", true, true, "Canlı Çiçekler", 1, 1 },
                    { 4, "fa-solid fa-leaf", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Yapay Çiçekler", "", true, true, "Yapay Çiçekler", 0, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
