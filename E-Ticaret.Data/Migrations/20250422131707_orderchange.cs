using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class orderchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderEmail",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderFirstName",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderLastName",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderPhone",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SenderEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SenderFirstName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SenderLastName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SenderPhone",
                table: "Orders");
        }
    }
}
