# PayTR kurulum notları

## PayTR panel ayarı

PayTR Mağaza Paneli > Destek & Kurulum > Ayarlar alanındaki **Bildirim URL** değeri:

`https://www.detaycicek.com/Order/PayTrCallback`

Bu adres dışarıdan erişilebilir olmalı, `POST` kabul etmeli ve başarılı işlemden sonra yalnızca `OK` yanıtı vermelidir. Başarılı/başarısız müşteri dönüş adresleri ödeme isteği sırasında uygulama tarafından otomatik gönderilir:

- Başarılı dönüş: `https://www.detaycicek.com/Order/PaymentSuccess`
- Başarısız dönüş: `https://www.detaycicek.com/Order/PaymentFailed`

Sipariş durumu dönüş sayfalarında değil, imzası doğrulanan Bildirim URL isteğinde değiştirilir.

## Sunucu ortam değişkenleri

Gerçek değerleri kaynak koda veya `appsettings.json` dosyasına yazmayın. Sunucuda aşağıdaki ortam değişkenlerini tanımlayın:

```text
PayTR__MerchantId=...
PayTR__MerchantKey=...
PayTR__MerchantSalt=...
PayTR__SiteUrl=https://www.detaycicek.com
PayTR__TestMode=1
PayTR__DebugOn=1
```

Testler tamamlanıp PayTR mağazası canlıya alındıktan sonra:

```text
PayTR__TestMode=0
PayTR__DebugOn=0
```

Taksit ayarları isteğe bağlıdır:

```text
PayTR__NoInstallment=0
PayTR__MaxInstallment=0
```

`NoInstallment=1` yalnızca tek çekim sunar. `MaxInstallment=0`, PayTR hesabının ve mevzuatın izin verdiği en yüksek taksit sayısını kullanır.

## Canlıya geçiş kontrolü

1. Alan adının HTTPS sertifikasını ve Bildirim URL'nin internetten erişilebilir olduğunu doğrulayın.
2. Önce PayTR test modunda başarılı ve başarısız ödeme deneyin.
3. Siparişin yalnızca callback sonrasında onaylandığını ve yinelenen callback'in ikinci kez e-posta/Telegram göndermediğini kontrol edin.
4. Yönetim panelinden uygun bir test siparişinde iade akışını doğrulayın.
5. PayTR onayından sonra test ve debug modlarını kapatın.
