using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Ticaret.Data.Configurations
{
    public class ContentPageConfiguration : IEntityTypeConfiguration<ContentPage>
    {
        public void Configure(EntityTypeBuilder<ContentPage> builder)
        {
            builder.Property(x => x.Key).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.BodyHtml).IsRequired();
            builder.HasIndex(x => x.Key).IsUnique();

            builder.HasData(
                new ContentPage
                {
                    Id = 1,
                    Key = ContentPageKeys.About,
                    Title = "Hakkımızda",
                    BodyHtml = """
                        <h2>Hikayemiz</h2>
                        <p>Detay Çiçekçilik 1992 yılında kurulmuştur. Firmamız 1992 yılından beri sizlere en iyi ve en kaliteli hizmeti sunmak için çalışmaktadır.</p>
                        <h3>Misyonumuz</h3>
                        <p>Müşterilerimize en kaliteli ürünleri en iyi hizmetle sunmak ve onların hayatına değer katmak.</p>
                        <h3>Vizyonumuz</h3>
                        <p>Sektöründe lider, global ölçekte tanınan bir marka olmak.</p>
                        <h2>Değerlerimiz</h2>
                        <p>Güven, yenilikçilik, şeffaf iletişim ve sürdürülebilirlik.</p>
                        """
                },
                new ContentPage
                {
                    Id = 2,
                    Key = ContentPageKeys.PrivacySecurity,
                    Title = "Gizlilik ve Güvenlik",
                    BodyHtml = """
                        <p>Sipariş formu sayfamız SSL teknolojisi ile şifrelenmiştir. Gizlilik arz eden kredi kartı bilgileriniz doğrudan ödeme yapılan bankaya iletilir ve üçüncü kişilerce görüntülenemez.</p>
                        <p>Kart bilgileriniz hiçbir şekilde veritabanımızda kayıt altında tutulmamaktadır. Girmiş olduğunuz kredi kartı bilgilerinin doğruluğu size aittir.</p>
                        """
                },
                new ContentPage
                {
                    Id = 3,
                    Key = ContentPageKeys.DeliveryWarranty,
                    Title = "Teslimat ve Garanti",
                    BodyHtml = """
                        <h2>Teslimat Şartları</h2>
                        <p>Ürünlerimiz özenle hazırlanarak belirtilen teslimat aralığında alıcıya ulaştırılır. Özel günlerde yoğunluk nedeniyle saat garantisi verilemeyebilir.</p>
                        <h3>Tazelik Garantisi</h3>
                        <p>Tüm çiçeklerimiz günlük mezat ürünleri olup tazelik garantimiz altındadır. Mevsimsel veya lojistik nedenlerle aynı kalite ve değerde benzer ürün ya da renk kullanılabilir.</p>
                        <h3>Adres ve Teslim Tarihi Değişiklikleri</h3>
                        <p>Adres ve teslim tarihi değişiklikleri sipariş yola çıkmadan önce yapılabilir. Sipariş ancak hazırlanmadan veya yola çıkmadan iptal edilebilir.</p>
                        """
                });
        }
    }
}
