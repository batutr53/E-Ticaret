using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class v444 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryTimeRangeId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryTimeRanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RangeText = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTimeRanges", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeliveryTimeRanges",
                columns: new[] { "Id", "EndTime", "IsActive", "RangeText", "StartTime" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 13, 30, 0, 0), true, "09:00 - 13:30", new TimeSpan(0, 9, 0, 0, 0) },
                    { 2, new TimeSpan(0, 17, 0, 0, 0), true, "12:30 - 17:00", new TimeSpan(0, 12, 30, 0, 0) },
                    { 3, new TimeSpan(0, 18, 0, 0, 0), true, "13:00 - 18:00", new TimeSpan(0, 13, 0, 0, 0) },
                    { 4, new TimeSpan(0, 22, 0, 0, 0), true, "17:00 - 22:00", new TimeSpan(0, 17, 0, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryTimeRangeId",
                table: "Orders",
                column: "DeliveryTimeRangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliveryTimeRanges_DeliveryTimeRangeId",
                table: "Orders",
                column: "DeliveryTimeRangeId",
                principalTable: "DeliveryTimeRanges",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliveryTimeRanges_DeliveryTimeRangeId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliveryTimeRanges");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveryTimeRangeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryTimeRangeId",
                table: "Orders");
        }
    }
}
