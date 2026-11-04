# 4 Kasım 2026 — PayTR iptal, Telegram teşhisi ve görsel sığdırma

## PayTR

- PayTR'nin resmi İade API sözleşmesi: https://dev.paytr.com/iade-api . Tahsil edilmiş sipariş iptali için `https://www.paytr.com/odeme/iade` kullanılır.
- Ayrı İptal/İade butonları, liste durum değiştirme ve düzenleme kaydı aynı `OrderStatusService` akışına gider. Eskiden son iki yol sadece veritabanı durumunu değiştiriyordu.
- Tutar istemciden alınmaz: kayıtlı `SubTotal + DeliveryFree`, TL cinsinden iki ondalık basamakla ve invariant kültürle gönderilir. İmza resmi HMAC-SHA256 sözleşmesine uygundur.
- Başarı yanıtında `merchant_oid` ve `return_amount` eşleşmesi doğrulanır. Yalnızca doğrulanmış başarıda Cancel/Refund kaydedilir. Açık ret eski durumu korur.
- Gönderimden önce koşullu atomik UPDATE ile `RefundProcessing = 8` kaydedilir. Mevcut enum numaraları değiştirilmedi. Tekrar/eşzamanlı istek yeni iade gönderemez. İstemci bağlantısının kapanması sonucu kaydetmeyi iptal etmez.
- Zaman aşımı, ağ hatası, beklenmeyen yanıt veya gönderim sırasında uygulamanın kapanması halinde durum kontrol bekler. Otomatik tekrar yapılmaz; yönetim paneli uyarı verir. **Bu durumun otomatik uzlaştırma ekranı yoktur:** teknik destek önce PayTR panelinde OID/tutar üzerinden iade sonucunu doğrulamalıdır. Sonuç kesinleşmeden veritabanı durumu sıfırlanmamalıdır.
- Ödeme kimliği ve tahsilat tutarı düzenlemede korunur; ödeme onayı elle oluşturulamaz. Sipariş silme ödeme/iade geçmişini kaybetmemek için engellendi.
- Eski tarihli ödenmiş siparişlerde de iptal/iade kullanılabilir; takvim günü kısıtı kaldırıldı. Pending/PaymentFailed/Cancel/Refund siparişlerden yeni iade gönderilmez.
- Mevcut OrderStatus integer kolonuna enum değeri eklendi; şema değişmedi, bu çalışma için migration gerekmez. Eski uygulama sürümüne geri dönüşte RefundProcessing durumunu tanımayacağı dikkate alınmalıdır.
- Gerçek müşteri siparişine iptal/iade gönderilmedi. Gerçek PayTR başarı testi yapılmadı; HTTP sahte yanıtlarıyla test edildi.

## Telegram — doğrulanmış engel

- Yerel `appsettings.json` ve `appsettings.Development.json` yapılandırmalarından ayrı ayrı Telegram `getMe` ve `getChat` çağrıları yapıldı. Dördü de **HTTP 401 / Unauthorized** döndü. Bot anahtarları geçerli değil. Anahtarlar çıktıya veya bu belgeye yazılmadı.
- Hedef DB salt okunur kontrolünde migration eksiği yok. Bir gönderilmemiş kuyruk kaydı var ve 10 denemeye ulaşmış. Dolayısıyla otomatik yeniden deneme sınırında bekliyor.
- Kullanıcının ayrıca paylaştığı bot anahtarı da getMe ile test edildi ve 401 Unauthorized döndü. Doğrulama başarısız olduğu için test mesajı gönderilmedi. Anahtar repoya kaydedilmedi.
- Kontrol edilen ilk anahtarlar yerel proje ayarlarıdır; canlı sunucunun farklı ortam değişkeni/secret override değeri bu incelemede okunmadı.
- Telegram hataları artık sabit güvenli açıklamalarla 401/403/400/429 olarak ayrılır. Ham API yanıtı, bot token veya token içeren URL kaydedilmez.
- `/Admin/Notifications` sayfasına yalnızca `getMe` ve `getChat` kullanan, mesaj göndermeyen bağlantı kontrolü eklendi. Başarılı kontrol mesaj gönderme yetkisinin kesin kanıtı değildir.
- Gereken işlem: BotFather üzerinden geçerli bot anahtarını edinip **gerçek sunucunun** `TelegramBot__BotToken` ayarını güncellemek; ardından uygulamayı yeniden başlatmak, bağlantı kontrolünü çalıştırmak ve başarısız kayıtta Tekrar Dene kullanmak. Yeni anahtar sohbete yazılmamalı veya repoya commit edilmemeli.
- İnceleme sırasında gerçek Telegram mesajı gönderilmedi ve eski bildirim tekrar tetiklenmedi.

## Görseller

- Kullanıcı tercihi: kartlar eşit ve tamamen dolu olacak; fotoğraf kenarlarından kırpılabilir.
- Canlı ana sayfa, ARANJMAN kategorisi ve BT Dzayn ürün detayı incelendi. İlk sorun bağlantı içindeki yüzde yüksekliğinin doğal fotoğraf yüksekliğine dönmesi ve kart dışına taşmasıydı.
- Güncel kart görsel alanı `clamp(240px, 40svh, 360px)` yüksekliğindedir. İç bağlantı ve fotoğraf absolute inset ile alana bağlanır. `object-fit:cover`, merkez hizalama ve sıfır padding ile kutu tamamen dolar. Fotoğrafın oranı korunur; taşan kenarlar kırpılır. Hover büyütmesi kapalıdır.
- Ürün detayı `contain` kullanır; fotoğrafın tamamı görünür. Detay görsel yüksekliği `clamp(280px, 60svh, 560px)` ile sınırlıdır.
- Masaüstü önizlemede ilk dört kart 279×360; 390×844 mobilde 325×338 ölçüldü. Her birinde cover doğrulandı. Canlı sayfanın herkese açık HTML'i üzerinde güncel yerel stylesheet ile kontrol yapıldı.
- Orijinal görsel dosyaları ve canlı DB değiştirilmedi. Canlı yayın yapılmadı.

## Doğrulama ve yayın durumu

- Release: 56 .NET testi başarılı (0 başarısız). Mevcut nullable ve paket uyarıları devam ediyor.
- İptal testleri: dört admin girişi, yol ücreti, eski sipariş, ret, belirsiz sonuç, tekrar/eşzamanlı çağrı, ödeme alanı değiştirme, yetkisiz durum geçişi, resmi imza ve Türkçe kültür altında ondalık tutar.
- Telegram testleri: güvenli belirgin hata, token sızdırmama, mesaj göndermeyen bağlantı kontrolü ve mevcut kuyruk/tekrar testleri.
- Kod yerel çalışma alanında. Commit/push/canlı yayın yapılmadı. Bu çalışma için hedef DB'ye yazılmadı.
