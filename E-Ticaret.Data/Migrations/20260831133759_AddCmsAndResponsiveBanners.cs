using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace E_Ticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCmsAndResponsiveBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayType",
                table: "Sliders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Desktop");

            migrationBuilder.AddColumn<int>(
                name: "OrderNo",
                table: "Sliders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContentPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BodyHtml = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobileBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Image = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OrderNo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileBanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Logo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    AccentColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                    table.CheckConstraint("CK_SiteSettings_Singleton", "\"Id\" = 1");
                });

            migrationBuilder.InsertData(
                table: "ContentPages",
                columns: new[] { "Id", "BodyHtml", "Key", "Title" },
                values: new object[,]
                {
                    { 1, "<h2>Hikayemiz</h2>\r\n<p>Detay Çiçekçilik 1992 yılında kurulmuştur. Firmamız 1992 yılından beri sizlere en iyi ve en kaliteli hizmeti sunmak için çalışmaktadır.</p>\r\n<h3>Misyonumuz</h3>\r\n<p>Müşterilerimize en kaliteli ürünleri en iyi hizmetle sunmak ve onların hayatına değer katmak.</p>\r\n<h3>Vizyonumuz</h3>\r\n<p>Sektöründe lider, global ölçekte tanınan bir marka olmak.</p>\r\n<h2>Değerlerimiz</h2>\r\n<p>Güven, yenilikçilik, şeffaf iletişim ve sürdürülebilirlik.</p>", "About", "Hakkımızda" },
                    { 2, "<p>Sipariş formu sayfamız SSL teknolojisi ile şifrelenmiştir. Gizlilik arz eden kredi kartı bilgileriniz doğrudan ödeme yapılan bankaya iletilir ve üçüncü kişilerce görüntülenemez.</p>\r\n<p>Kart bilgileriniz hiçbir şekilde veritabanımızda kayıt altında tutulmamaktadır. Girmiş olduğunuz kredi kartı bilgilerinin doğruluğu size aittir.</p>", "PrivacySecurity", "Gizlilik ve Güvenlik" },
                    { 3, "<h2>Teslimat Şartları</h2>\r\n<p>Ürünlerimiz özenle hazırlanarak belirtilen teslimat aralığında alıcıya ulaştırılır. Özel günlerde yoğunluk nedeniyle saat garantisi verilemeyebilir.</p>\r\n<h3>Tazelik Garantisi</h3>\r\n<p>Tüm çiçeklerimiz günlük mezat ürünleri olup tazelik garantimiz altındadır. Mevsimsel veya lojistik nedenlerle aynı kalite ve değerde benzer ürün ya da renk kullanılabilir.</p>\r\n<h3>Adres ve Teslim Tarihi Değişiklikleri</h3>\r\n<p>Adres ve teslim tarihi değişiklikleri sipariş yola çıkmadan önce yapılabilir. Sipariş ancak hazırlanmadan veya yola çıkmadan iptal edilebilir.</p>", "DeliveryWarranty", "Teslimat ve Garanti" }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "AccentColor", "Logo", "PrimaryColor" },
                values: new object[] { 1, "#236B43", null, "#888888" });

            migrationBuilder.CreateIndex(
                name: "IX_Sliders_DisplayType_OrderNo",
                table: "Sliders",
                columns: new[] { "DisplayType", "OrderNo" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentPages_Key",
                table: "ContentPages",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MobileBanners_OrderNo",
                table: "MobileBanners",
                column: "OrderNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPages");

            migrationBuilder.DropTable(
                name: "MobileBanners");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Sliders_DisplayType_OrderNo",
                table: "Sliders");

            migrationBuilder.DropColumn(
                name: "DisplayType",
                table: "Sliders");

            migrationBuilder.DropColumn(
                name: "OrderNo",
                table: "Sliders");
        }
    }
}
