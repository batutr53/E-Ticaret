using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class v34 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "ClassImage", "CreatedDate", "Description", "Image", "IsActive", "IsTopMenu", "Name", "OrderNo", "ParentId" },
                values: new object[,]
                {
                    { 5, "fa-solid fa-gift", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "KADINLAR GÜNÜ ÇİÇEKLERİ", "", true, false, "KADINLAR GÜNÜ ÇİÇEKLERİ", 0, 1 },
                    { 6, "fa-solid fa-leaf", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "ARANJMANLAR", "", true, false, "ARANJMANLAR", 0, 1 },
                    { 7, "fa-solid fa-leaf", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "ORKİDELER", "", true, false, "ORKİDELER", 0, 1 },
                    { 8, "fa-solid fa-fan", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "ÇİÇEK BUKETLERİ", "", true, false, "ÇİÇEK BUKETLERİ", 0, 1 },
                    { 9, "fa-solid fa-seedling", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Canlı Aranjmanlar", "", true, false, "Canlı Aranjmanlar", 0, 1 },
                    { 10, "fa-solid fa-heart", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Gül Aranjmanları", "", true, false, "Gül Aranjmanları", 0, 1 },
                    { 11, "fa-solid fa-leaf", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Yapay Aranjmanlar", "", true, false, "Yapay Aranjmanlar", 0, 1 },
                    { 12, "fa-solid fa-leaf", new DateTime(2024, 4, 1, 10, 5, 0, 0, DateTimeKind.Utc), "Yapay İç Dekorasyon", "", true, false, "Yapay İç Dekorasyon", 0, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12);
        }
    }
}
