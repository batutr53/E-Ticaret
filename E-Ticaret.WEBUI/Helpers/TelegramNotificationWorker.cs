using E_Ticaret.Data;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Helpers;

public class TelegramNotificationWorker(IServiceScopeFactory scopeFactory, ILogger<TelegramNotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessPendingAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception)
            {
                logger.LogError("Telegram bildirim kuyruğu işlenemedi. Veritabanı ve migration durumunu kontrol edin.");
            }
            try { if (!await timer.WaitForNextTickAsync(stoppingToken)) break; }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var telegram = scope.ServiceProvider.GetRequiredService<TelegramHelper>();
        if (!telegram.IsConfigured) return;
        var now = DateTime.UtcNow;
        var ids = await db.TelegramNotifications.AsNoTracking()
            .Where(x => x.SentAt == null && x.Attempts < 10 && x.NextAttemptAt <= now)
            .OrderBy(x => x.Id).Select(x => x.Id).Take(20).ToListAsync(cancellationToken);
        foreach (var id in ids)
        {
            var claimTime = DateTime.UtcNow;
            // Atomic lease prevents multiple application instances from claiming the same job.
            var claimed = await db.TelegramNotifications
                .Where(x => x.Id == id && x.SentAt == null && x.Attempts < 10 && x.NextAttemptAt <= claimTime)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.NextAttemptAt, claimTime.AddMinutes(2))
                    .SetProperty(x => x.LastError, (string?)null)
                    .SetProperty(x => x.Attempts, x => x.Attempts + 1), cancellationToken);
            if (claimed == 0) continue;
            var job = await db.TelegramNotifications.SingleAsync(x => x.Id == id, cancellationToken);
            try
            {
                var order = await db.Orders.AsNoTracking().SingleAsync(x => x.Id == job.OrderId, cancellationToken);
                await telegram.SendTelegramOrderMessage(order.Id, $"Yeni ödenmiş sipariş: {order.Oid}", cancellationToken);
                job.SentAt = DateTime.UtcNow;
                job.LastError = null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (TelegramDeliveryException exception)
            {
                job.LastError = exception.Message;
                job.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Min(60, Math.Pow(2, job.Attempts)));
                logger.LogWarning("Sipariş {OrderId} Telegram bildirimi reddedildi: {Reason}", job.OrderId, exception.Message);
            }
            catch (Exception)
            {
                job.LastError = "Gönderilemedi. Bot ayarlarını, sohbet erişimini ve sunucu bağlantısını kontrol edin.";
                job.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Min(60, Math.Pow(2, job.Attempts)));
                logger.LogWarning("Sipariş {OrderId} Telegram bildirimi gönderilemedi. Deneme: {Attempts}", job.OrderId, job.Attempts);
            }
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }
    }
}
