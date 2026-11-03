namespace E_Ticaret.Core.Entities;

// Persisted in the payment transaction so an application restart cannot lose the notification.
public class TelegramNotification
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Attempts { get; set; }
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string? LastError { get; set; }
}
