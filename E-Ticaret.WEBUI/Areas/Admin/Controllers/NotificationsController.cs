using E_Ticaret.Data;
using E_Ticaret.WEBUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class NotificationsController(DatabaseContext db, TelegramHelper telegram) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.IsConfigured = telegram.IsConfigured;
        return View(await db.TelegramNotifications.AsNoTracking().OrderByDescending(x => x.Id).Take(100).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckConnection(CancellationToken cancellationToken)
    {
        TempData["ConnectionResult"] = await telegram.CheckConnectionAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Retry(int id)
    {
        var now = DateTime.UtcNow;
        await db.TelegramNotifications.Where(x => x.Id == id && x.SentAt == null &&
            (x.NextAttemptAt <= now || x.LastError != null))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Attempts, 0).SetProperty(x => x.LastError, (string?)null)
                .SetProperty(x => x.NextAttemptAt, now));
        return RedirectToAction(nameof(Index));
    }
}
