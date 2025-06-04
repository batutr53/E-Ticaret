using E_Ticaret.Core.Entities;

namespace E_Ticaret.Core.Enums
{
    public static class OrderStatusExtensions
    {
        public static string ToText(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Beklemede",
                OrderStatus.Approved => "Onaylandı",
                OrderStatus.Ready => "Hazırlanıyor",
                OrderStatus.OnWay => "Yolda",
                OrderStatus.Completed => "Ödeme Tamamlandı",
                OrderStatus.Cancel => "İptal Edildi",
                OrderStatus.Refund => "İade Edildi",
                OrderStatus.PaymentFailed => "Ödeme Başarısız",
                _ => "Bilinmiyor"
            };
        }

    }
}
