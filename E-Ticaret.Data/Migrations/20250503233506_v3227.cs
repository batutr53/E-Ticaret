using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class v3227 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "ProductImages",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "ProductImages");
        }
    }
}
