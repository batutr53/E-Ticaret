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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int orderId, CancellationToken cancellationToken)
        {
            TempData["Message"] = await new OrderStatusService(_context, _payTrService)
                .ChangeAsync(orderId, OrderStatus.Refund, cancellationToken);
            return RedirectToAction("Details", new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int orderId, CancellationToken cancellationToken)
        {
            TempData["Message"] = await new OrderStatusService(_context, _payTrService)
                .ChangeAsync(orderId, OrderStatus.Cancel, cancellationToken);
            return RedirectToAction("Details", new { id = orderId });
        }

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
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.Id) return NotFound();
            var existing = await _context.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            if (existing == null) return NotFound();
            if (!Enum.IsDefined(order.OrderStatus))
                ModelState.AddModelError("OrderStatus", "Geçersiz sipariş durumu.");
            if (existing.OrderStatus == OrderStatus.RefundProcessing)
                ModelState.AddModelError("", "PayTR iade sonucu kontrol edilene kadar sipariş değiştirilemez.");
            if (existing.Oid != order.Oid || existing.TxnNo != order.TxnNo || existing.SubTotal != order.SubTotal || existing.DeliveryFree != order.DeliveryFree)
                ModelState.AddModelError("", "Ödeme numarası ve tahsil edilen tutar değiştirilemez.");
            if (order.OrderStatus != existing.OrderStatus && order.OrderStatus is not
                (OrderStatus.Cancel or OrderStatus.Refund or OrderStatus.Ready or OrderStatus.OnWay or OrderStatus.Completed))
                ModelState.AddModelError("OrderStatus", "Ödeme durumları elle değiştirilemez.");
            if (ModelState.IsValid)
            {
                var deliveryDate = order.DeliveryDate.HasValue
                    ? DateTime.SpecifyKind(order.DeliveryDate.Value, DateTimeKind.Utc) : (DateTime?)null;
                // Update only editable customer/delivery fields, never payment identifiers, totals or status.
                var changed = await _context.Orders.Where(x => x.Id == id && x.OrderStatus == existing.OrderStatus)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.FirstName, order.FirstName).SetProperty(x => x.LastName, order.LastName)
                        .SetProperty(x => x.Phone, order.Phone).SetProperty(x => x.AddresLine, order.AddresLine)
                        .SetProperty(x => x.SenderFirstName, order.SenderFirstName).SetProperty(x => x.SenderLastName, order.SenderLastName)
                        .SetProperty(x => x.SenderPhone, order.SenderPhone).SetProperty(x => x.SenderEmail, order.SenderEmail)
                        .SetProperty(x => x.CardMessage, order.CardMessage).SetProperty(x => x.Description, order.Description)
                        .SetProperty(x => x.DeliveryDate, deliveryDate).SetProperty(x => x.DeliveryTimeRangeId, order.DeliveryTimeRangeId));
                TempData["Message"] = changed == 0 ? "Sipariş başka bir işlemde değişti; sayfayı yenileyin."
                    : await new OrderStatusService(_context, _payTrService).ChangeAsync(id, order.OrderStatus);
                return RedirectToAction("Details", new { id });
            }
            ViewBag.DeliveryTimeRanges = new SelectList(await _context.DeliveryTimeRanges
                .Where(x => x.IsActive).OrderBy(x => x.StartTime).ToListAsync(), "Id", "RangeText", order.DeliveryTimeRangeId);
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
                TempData["Message"] = "Ödeme ve iade geçmişi korunmalıdır. Siparişi silmek yerine iptal/iade işlemini kullanın.";
                return RedirectToAction("Details", new { id });
            }

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
            if (!Enum.TryParse<OrderStatus>(orderStatus, out var target) || !Enum.IsDefined(target))
                TempData["Message"] = "Geçersiz sipariş durumu.";
            else
                TempData["Message"] = await new OrderStatusService(_context, _payTrService).ChangeAsync(orderId, target);
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
