# Mobil görsel düzeltmesi — 5 Kasım 2026

- Yalnızca style.css içinde max-width:767px mobil kuralları eklendi. Masaüstü kuralları değiştirilmedi.
- Kart yüksekliğinin telefon yüksekliğine bağlı 40svh hesabı mobilde geçersiz kılındı. Kart genişliğine bağlı 10:13 oranı kullanılır. Kullanıcının onayladığı cover/merkez kırpma korunur.
- Ürün detayındaki 300px minimum sütun genişliği mobilde kaldırıldı; resim alanı genişliğe göre 10:13 oranında ve en fazla 560px, contain kullanır.
- Mobil slider resimleri sabit yüksekliğe zorlanmadan doğal oranıyla gösterilir; hover büyütmesi devre dışıdır.
- Güncel CSS ile yerel önizlemede kategori 360,390,480,540,767px; ürün detayı 320,390,540px genişliklerde kontrol edildi. Yatay taşma yok. 1920px masaüstü kartı 279x360 olarak korunuyor.
- Canlı ana sayfanın güncel herkese açık HTML'i yerel CSS ile kontrol edildi: 390px ekranda kampanya resimleri 353px genişlikte ve kaynak oranını koruyor.
- Önizlemeler canlı HTML + yerel güncel CSS kullanır; canlı yayın kanıtı değildir. Commit/push/yayın yapılmadı. Migration yok.
