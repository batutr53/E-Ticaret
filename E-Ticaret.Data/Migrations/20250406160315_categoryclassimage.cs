using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class categoryclassimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassImage",
                table: "Categories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassImage",
                table: "Categories");
        }
    }
}
