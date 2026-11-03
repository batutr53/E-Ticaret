# İl / ilçe / mahalleye göre teslimat ücreti

Bu düzenleme, önceki serbest bölge seçimi yaklaşımının yerini alır. Bölge etiketi fiyat eşleştirmesinde kullanılmaz; yönetim etiketi olarak kalır.

## Kullanım

1. Yönetimde **Teslimat Bölgeleri → Yeni Teslimat Bölgesi** açılır.
2. Yönetim etiketi, ücret ve aktiflik belirlenir. İl ve ilçe listelerinden mahalleler seçilir. Farklı ilçelerden seçimler aynı bölgede tutulabilir.
3. Seçilen her mahalle bu ücret bölgesine bağlanır. Başka bir bölgeye bağlı mahalle tekrar bağlanamaz. Taşımak için önce eski bölgeden çıkarılmalıdır. Son mahalleyi bırakmak için eski bölge pasif yapılabilir.
4. Müşteri il, ilçe ve mahalleyi listeden seçer. Yalnızca aktif bir bölgeye bağlanmış yerler listelenir. Sokak, bina ve daire bilgisi ayrı açık adres alanına yazılır.
5. Mahallenin bağlı olduğu bölgenin ücreti özete ve PayTR tutarına yansır. Sunucu il–ilçe–mahalle kimliklerinin ilişkisini ve aktifliği yeniden denetler. Gönderilen serbest il/bölge adı veya `DeliveryFree` ücreti kullanılmaz.
6. Ekrandaki ücret teklifi ile veritabanındaki ücret farklıysa sipariş oluşturulmaz; müşteriden adres seçimini yenilemesi istenir.

Tanımlanmayan veya kapalı mahallelerde ücretsiz teslimata düşülmez; sipariş engellenir. Hiç mahalle bağlanmamışsa ödeme açılamaz. Ücretsiz teslimat da açıkça bir bölgeye bağlanıp 0 TL olarak tanımlanmalıdır.

İlçe değişince mahalle ve eski ücret sıfırlanır. Liste yüklenirken veya yüklenemezse adım doğrulaması geçmez. Gecikmiş eski yanıtların yeni adres seçimini değiştirmesi engellenir. Aynı adlı mahalleler farklı yerel belediyelere bağlıysa etiketlerinde bu ad da gösterilir.

## Referans verisi

81 il, 973 ilçe ve 32.279 mahalle sabit kaynak kimlikleriyle yerel veritabanındadır. Sipariş sırasında dış adres API'sine istek yapılmaz.

Kaynak: [TurkiyeAPI](https://github.com/ubeydeozdmr/turkiye-api), 2025 veri kümesi, commit `a0ad8351cb52153e7a92aa7ba9a5bc1e3294da8d`. MIT lisansı ve kaynak dosyaların SHA-256 bilgileri `E-Ticaret.Data/ReferenceData/` altında tutulur. Bu, kaynağın sabit bir kopyasıdır; canlı NVI doğrulaması değildir. İdari değişiklikler için ileride kontrollü veri güncellemesi gerekir. Köy veri kümesi bu kapsama alınmadı.

Mahalle seçimi, müşterinin beyan ettiği mahalleye doğru ücret uygular. Yazılan sokak/bina bilgisinin gerçekten o mahallede bulunduğu coğrafi olarak doğrulanmaz.

## Sipariş geçmişi

Yeni siparişe il, ilçe ve mahalle kimlikleri; ilçe/mahalle adları ve o anki bölge adı/ücreti kaydedilir. Bölge etiketinin veya ücretinin sonradan değiştirilmesi eski siparişleri değiştirmez. Eski siparişlere tahmini mahalle ataması yapılmadı.

## Uygulanan veritabanı geçişi

- Migration: `20261103140738_NeighborhoodDeliveryMapping`.
- Script: `20261103_neighborhood_mapping.sql`.
- Önce güncel tam yedek alındı: `C:\Users\maste\.codex\backups\dtyc\20261103-before-neighborhood-mapping\database.dump`.
- SHA-256: `A569DEEDA66F841944AACE968F38F2339237AB65BC57509254D61CAA28930919`.
- Yedek dış ağa kapalı geçici PostgreSQL 17 ortamına geri yüklendi ve geçiş önce bu kopyada denendi.
- Hedefte geçiş transaction içinde uygulandı. Önceden var olan 25 tablonun satır sayıları ve içerik parmak izleri önce/sonra aynı kaldı. Geçiş anındaki 28 sipariş ve 536 ürün korundu.
- Ardından ayrı salt okunur bağlantıyla il/ilçe/mahalle sayıları, bilinen bir adresin Türkçe adları ve hiyerarşisi, yeni sipariş alanları doğrulandı. Bekleyen migration sayısı 0.
- Gerçek teslimat ücretleri bilinmediği için **hiçbir mahalleye tahmini ücret bölgesi atanmadı**. Bölge eşleştirmeleri yönetimden yapılmalıdır.
- Ayrıntılı rapor yedek klasöründeki `migration-verification.json` dosyasındadır. Geri yükleme için kullanılan geçici ortam kaldırıldı.

## Doğrulama ve yayın durumu

33 .NET testi ve 6 JavaScript testi geçti. Gerçek Razor sipariş ve yönetim formları ile ilçe/mahalle sorguları test edildi. Çakışan mahalle ataması, bilinmeyen kimlik, tutarsız il–ilçe ilişkisi, kapalı/tanımsız mahalle, açıkça ücretsiz mahalle, değişen ücret teklifi ve eski sipariş bilgilerinin korunması kapsanır.

```powershell
dotnet test E-Ticaret.Tests/E-Ticaret.Tests.csproj -c Release
node --test E-Ticaret.Tests/frontend.test.cjs E-Ticaret.Tests/address-frontend.test.cjs
```

Yerelde açık Debug uygulaması dosyaları kilitlediği için açık uygulama kapatılmadan ayrı Release çıktısı kullanıldı. Mevcut nullable ve paket uyarıları önceki notlarda açıklanmıştır. Tarayıcıda görsel/mobil QA ve gerçek PayTR/Telegram işlemi yapılmadı.

**Veritabanı güncellendi; uygulama kodu henüz sunucuya yayımlanmadı.** Yeni sürüm yayına alınmadan önce işletmenin teslimat yaptığı mahalleler ve ücretleri tanımlanmalıdır. Çalışan uygulama yeni sürümle yeniden başlatılmalıdır.
