using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Payments;

public sealed class OrderStatusService(DatabaseContext db, IPayTrService payTr)
{
    public async Task<string> ChangeAsync(int orderId, OrderStatus target, CancellationToken cancellationToken = default)
    {
        var order = await db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == orderId, cancellationToken);
        if (order == null) return "Sipariş bulunamadı.";
        if (!Enum.IsDefined(target)) return "Geçersiz sipariş durumu.";
        if (order.OrderStatus == OrderStatus.RefundProcessing)
            return "PayTR iade sonucu kontrol edilmeli. Tekrar iade gönderilmedi. PayTR panelinden sonucu doğrulamak için teknik destekle iletişime geçin.";
        if (order.OrderStatus == target) return "Sipariş zaten bu durumda; tekrar işlem yapılmadı.";
        var paid = order.OrderStatus is OrderStatus.Approved or OrderStatus.Ready or OrderStatus.OnWay or OrderStatus.Completed;
        if (!paid) return "Bu sipariş için ödeme onayı bulunmuyor veya sipariş zaten iptal/iade edilmiş.";

        if (target is not (OrderStatus.Cancel or OrderStatus.Refund))
        {
            if (target is not (OrderStatus.Ready or OrderStatus.OnWay or OrderStatus.Completed))
                return "Ödeme durumları elle değiştirilemez.";
            var changed = await db.Orders.Where(x => x.Id == orderId && x.OrderStatus == order.OrderStatus)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.OrderStatus, target), cancellationToken);
            return changed == 1 ? "Sipariş durumu başarıyla güncellendi." : "Sipariş başka bir işlemde değişti; sayfayı yenileyin.";
        }

        var amount = order.GetTotal();
        if (string.IsNullOrWhiteSpace(order.Oid) || amount <= 0 || decimal.Round(amount, 2) != amount)
            return "PayTR sipariş numarası veya iade tutarı geçersiz; işlem yapılmadı.";

        // Durable compare-and-swap: concurrent requests and process restarts cannot send a second refund.
        var claimed = await db.Orders.Where(x => x.Id == orderId && x.OrderStatus == order.OrderStatus
                && x.Oid == order.Oid && x.SubTotal == order.SubTotal && x.DeliveryFree == order.DeliveryFree)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.OrderStatus, OrderStatus.RefundProcessing), cancellationToken);
        if (claimed != 1) return "Sipariş başka bir işlemde değişti; tekrar iade gönderilmedi. Sayfayı yenileyin.";

        PayTrRefundResult result;
        try
        {
            // Request disconnect must not prevent saving the outcome of a financial operation.
            result = await payTr.RefundAsync(order.Oid, amount, CancellationToken.None);
        }
        catch (Exception)
        {
            return "PayTR iade sonucu belirsiz. Tekrar işlem yapılmayacak; PayTR panelinden kontrol için teknik destekle iletişime geçin.";
        }
        if (result.IsUncertain)
            return "PayTR iade sonucu belirsiz. Tekrar işlem yapılmayacak; PayTR panelinden kontrol için teknik destekle iletişime geçin.";

        await db.Orders.Where(x => x.Id == orderId && x.OrderStatus == OrderStatus.RefundProcessing)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.OrderStatus, result.IsSuccess ? target : order.OrderStatus), CancellationToken.None);
        return result.IsSuccess
            ? "PayTR tam tutar iade talebini kabul etti; sipariş durumu güncellendi."
            : $"PayTR iade talebini reddetti; sipariş durumu korunuyor. {result.ErrorCode} {result.ErrorMessage}";
    }
}
