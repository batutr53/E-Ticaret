using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Contacts");

            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "Contacts",
                newName: "NameSurname");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameSurname",
                table: "Contacts",
                newName: "Surname");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Contacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
