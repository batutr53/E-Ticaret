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
                OrderStatus.PaymentFailed => "Ödeme Başarısız",
                OrderStatus.Completed => "Ödeme Tamamlandı",
                OrderStatus.OnWay => "Yolda",
                _ => "Bilinmiyor"
            };
        }
    }
}
