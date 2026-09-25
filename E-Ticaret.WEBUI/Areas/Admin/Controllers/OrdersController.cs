using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using Microsoft.AspNetCore.Authorization;
using E_Ticaret.WEBUI.Payments;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IPayTrService _payTrService;
        public OrdersController(DatabaseContext context, IPayTrService payTrService)
        {
            _context = context;
            _payTrService = payTrService;
        }

        #region Refund
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .Include(o => o.DeliveryTimeRange)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == orderId, cancellationToken);

            if (order == null)
            {
                TempData["Message"] = "Sipariş Bulunamadı.";
                return RedirectToAction("Details", new { id = orderId });
            }

            if (string.IsNullOrWhiteSpace(order.Oid))
            {
                TempData["Message"] = "Siparişin PayTR sipariş numarası bulunamadı.";
                return RedirectToAction("Details", new { id = orderId });
            }

            if (order.OrderStatus != OrderStatus.Completed)
            {
                TempData["Message"] = "Yalnızca tamamlanmış siparişler iade edilebilir.";
                return RedirectToAction("Details", new { id = orderId });
            }

            var result = await _payTrService.RefundAsync(order.Oid, order.GetTotal(), cancellationToken);
            if (result.IsSuccess)
            {
                order.OrderStatus = OrderStatus.Refund;
                await _context.SaveChangesAsync(cancellationToken);
                TempData["Message"] = "PayTR iade işlemi başarıyla tamamlandı.";
            }
            else
            {
                TempData["Message"] = $"PayTR iade hatası{(string.IsNullOrWhiteSpace(result.ErrorCode) ? "" : $" ({result.ErrorCode})")}: {result.ErrorMessage}";
            }

            return RedirectToAction("Details", new { id = orderId });
        }

        #endregion

        #region Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
            if (order == null)
            {
                TempData["Message"] = "Sipariş bulunamadı.";
                return RedirectToAction("Details", new { id = orderId });
            }

            if (string.IsNullOrWhiteSpace(order.Oid))
            {
                TempData["Message"] = "Siparişin PayTR sipariş numarası bulunamadı.";
                return RedirectToAction("Details", new { id = orderId });
            }

            if (order.OrderDate.Date < DateTime.Now.Date ||
                order.OrderStatus is not (OrderStatus.Approved or OrderStatus.Ready))
            {
                TempData["Message"] = "Bu sipariş iptal işlemine uygun değil.";
                return RedirectToAction("Details", new { id = orderId });
            }

            var result = await _payTrService.RefundAsync(order.Oid, order.GetTotal(), cancellationToken);
            if (result.IsSuccess)
            {
                order.OrderStatus = OrderStatus.Cancel;
                await _context.SaveChangesAsync(cancellationToken);
                TempData["Message"] = "PayTR üzerinden iptal/iade işlemi başarıyla tamamlandı.";
            }
            else
            {
                TempData["Message"] = $"PayTR iptal/iade hatası{(string.IsNullOrWhiteSpace(result.ErrorCode) ? "" : $" ({result.ErrorCode})")}: {result.ErrorMessage}";
            }

            return RedirectToAction("Details", new { id = orderId });
        }



        #endregion

        // GET: Admin/Orders
        public async Task<IActionResult> Index(
           E_Ticaret.Core.Entities.OrderStatus? status,
           string? receiver, string? sender,
           DateTime? startDate, DateTime? endDate,
           int page = 1, int pageSize = 10)
        {
            var query = _context.Orders
                .Include(x => x.DeliveryTimeRange)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(x => x.OrderStatus == status.Value);

            if (!string.IsNullOrWhiteSpace(receiver))
            {
                var receiverLower = receiver.ToLower();
                query = query.Where(x =>
                    x.FirstName.ToLower().Contains(receiverLower) ||
                    x.LastName.ToLower().Contains(receiverLower) ||
                    x.Phone.ToLower().Contains(receiverLower));
            }

            if (!string.IsNullOrWhiteSpace(sender))
            {
                var senderLower = sender.ToLower();
                query = query.Where(x =>
                    x.SenderFirstName.ToLower().Contains(senderLower) ||
                    x.SenderLastName.ToLower().Contains(senderLower) ||
                    x.SenderPhone.ToLower().Contains(senderLower));
            }

            if (startDate.HasValue)
                query = query.Where(x => x.OrderDate >= DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc));

            if (endDate.HasValue)
            {
                var endOfDayUtc = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
                query = query.Where(x => x.OrderDate <= endOfDayUtc);
            }

            var totalCount = await query.CountAsync();

            var pagedOrders = await query
                .OrderByDescending(x => x.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.ReceiverFilter = receiver;
            ViewBag.SenderFilter = sender;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(pagedOrders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.DeliveryTimeRange)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);

        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            return View();
        }



        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            {
                var order = await _context.Orders
                    .Include(o => o.DeliveryTimeRange)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                    return NotFound();

                ViewBag.DeliveryTimeRanges = new SelectList(
                    await _context.DeliveryTimeRanges
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.StartTime)
                        .ToListAsync(),
                    "Id", "RangeText", order.DeliveryTimeRangeId
                );

                return View(order);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (order.OrderDate.Kind != DateTimeKind.Utc)
                        order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);

                    if (order.DeliveryDate.HasValue && order.DeliveryDate.Value.Kind != DateTimeKind.Utc)
                        order.DeliveryDate = DateTime.SpecifyKind(order.DeliveryDate.Value, DateTimeKind.Utc);

                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DeliveryTimeRanges = new SelectList(
                await _context.DeliveryTimeRanges
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.StartTime)
                    .ToListAsync(),
                "Id", "RangeText", order.DeliveryTimeRangeId
            );

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string orderStatus)
        {
            if (string.IsNullOrEmpty(orderStatus))
            {
                TempData["Error"] = "Sipariş durumu boş olamaz!";
                return RedirectToAction("Index");
            }

            if (!Enum.TryParse(typeof(E_Ticaret.Core.Entities.OrderStatus), orderStatus, out var statusEnum))
            {
                TempData["Error"] = "Geçersiz sipariş durumu!";
                return RedirectToAction("Index");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Sipariş bulunamadı!";
                return RedirectToAction("Index");
            }

            order.OrderStatus = (E_Ticaret.Core.Entities.OrderStatus)statusEnum;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sipariş durumu başarıyla güncellendi!";
            return RedirectToAction("Index");
        }
    }
}
