using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class NeighborhoodDeliveryMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryDistrictId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryDistrictName",
                table: "Orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryNeighborhoodId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNeighborhoodName",
                table: "Orders",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryProvinceId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AddressProvinces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressProvinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddressDistricts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ProvinceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDistricts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressDistricts_AddressProvinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "AddressProvinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AddressNeighborhoods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    LocalityName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    DeliveryRegionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressNeighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressNeighborhoods_AddressDistricts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "AddressDistricts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddressNeighborhoods_DeliveryRegions_DeliveryRegionId",
                        column: x => x.DeliveryRegionId,
                        principalTable: "DeliveryRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDistricts_ProvinceId_Name",
                table: "AddressDistricts",
                columns: new[] { "ProvinceId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressNeighborhoods_DeliveryRegionId",
                table: "AddressNeighborhoods",
                column: "DeliveryRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressNeighborhoods_DistrictId_Name",
                table: "AddressNeighborhoods",
                columns: new[] { "DistrictId", "Name" });

            E_Ticaret.Data.ReferenceData.AddressReferenceCatalog.Insert(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressNeighborhoods");

            migrationBuilder.DropTable(
                name: "AddressDistricts");

            migrationBuilder.DropTable(
                name: "AddressProvinces");

            migrationBuilder.DropColumn(
                name: "DeliveryDistrictId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDistrictName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryNeighborhoodId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryNeighborhoodName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryProvinceId",
                table: "Orders");
        }
    }
}
