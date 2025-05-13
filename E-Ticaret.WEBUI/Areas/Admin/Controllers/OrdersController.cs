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
using E_Ticaret.WEBUI.Areas.Admin.Models;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Azure;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;
        public OrdersController(DatabaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        #region Refund
        public async Task<IActionResult> Refund(CancelRequestModel model)
        {
            var order = await _context.Orders
                .Include(o => o.DeliveryTimeRange)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == model.orderId);

            if (order == null)
            {
                TempData["Message"] = "Sipariş Bulunamadı.";
                return RedirectToAction("Details", new { id = model.orderId });
            }
            // TRPOS Tarafından verilen API bilgileri
            string publicKey = _config["TrPosAPI:PublicKey"];
            string apiKey = _config["TrPosAPI:APIKey"];
            string secretKey = _config["TrPosAPI:SecretKey"];
            //Her işlemde benzersiz gönderilmesi gereken rnd değeri
            string rnd = DateTime.Now.Ticks.ToString();
            // TRPOS tarafından verilen işlem numarası
            string txnNo = model.TxnNo;
            // Sizin tarafınızdan oluşturulan sipariş numarası
            string oid = model.Oid;
            // Müşteriye ait IP numarası
            double amount = (double)order.SubTotal;
            // Müşteriye ait IP numarası
            string userIP = model.UserIP ?? "1.1.1.1";
            // Response da geri dönecek ekstra data bilgileri.
            string extInfo = "";
            // HASH oluşturma fonksiyonu
            string birlestir = string.Concat(apiKey, publicKey, rnd, oid, txnNo,
            amount.ToString());
            HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(birlestir));
            string hash = Convert.ToBase64String(b);
            RefundRequestModel requestModel = new RefundRequestModel();
            requestModel.PublicKey = publicKey;
            requestModel.ApiKey = apiKey;
            requestModel.Rnd = rnd;
            requestModel.Hash = hash;
            requestModel.TxnNo = txnNo;
            requestModel.Oid = oid;
            requestModel.Amount = amount;
            requestModel.UserIP = userIP;
            requestModel.ExtInfo = extInfo;
            // TRPOS işlem iade POST URL
            string refundURL = _config["TrPosAPI:RefundURL"];
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await
                client.PostAsJsonAsync<RefundRequestModel>(refundURL, requestModel);

                if (response.IsSuccessStatusCode)
                {
                    var refundResponse =
                    JsonConvert.DeserializeObject<RefundResponseModel>(await
                    response.Content.ReadAsStringAsync()); // Refund Requestten alınan JSON cevap
                    if (refundResponse != null)
                    {
                        if (refundResponse.ResultCode == "0000" && hash ==
                        refundResponse.Hash)
                        {
                            TempData["Message"] = "İade işlemi başarıyla tamamlandı.";
                            return RedirectToAction("Details", new { id = model.orderId });
                        }
                        else
                        {
                            TempData["Message"] = refundResponse.ResultDetail;
                        }
                    }
                    else
                    {
                        TempData["Message"] = refundResponse.ResultDetail;
                    }
                }
                else
                {
                    TempData["Message"] = "Hata: Cevap alınamadı.";
                }
            }
            return RedirectToAction("Details", new { id = model.orderId });
        }
        #endregion

            #region Cancel
        public async Task<IActionResult> Cancel(CancelOrderModel model)
        {
            // TRPOS Tarafından verilen API bilgileri
            string publicKey = _config["TrPosAPI:PublicKey"];
            string apiKey = _config["TrPosAPI:APIKey"];
            string secretKey = _config["TrPosAPI:SecretKey"];
            //Her işlemde benzersiz gönderilmesi gereken rnd değeri
            string rnd = DateTime.Now.Ticks.ToString();
            // TRPOS tarafından verilen işlem numarası
            string txnNo = model.txnNo;
            // Sizin tarafınızdan oluşturulan sipariş numarası
            string oid = model.oid;
            // Müşteriye ait IP numarası
            string userIP = model.userIP ?? "1.1.1.1";
            // Response da geri dönecek ekstra data bilgileri
            string extInfo = "";
            // HASH oluşturma fonksiyonu
            string birlestir = string.Concat(apiKey.ToUpper(), publicKey, rnd, oid, txnNo);
            HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(birlestir));
            string hash = Convert.ToBase64String(b);
            CancelRequestModel requestModel = new CancelRequestModel();
            requestModel.PublicKey = publicKey;
            requestModel.ApiKey = apiKey;
            requestModel.Rnd = rnd;
            requestModel.Hash = hash;
            requestModel.TxnNo = txnNo;
            requestModel.UserIP = userIP;
            requestModel.Oid = oid;
            requestModel.ExtInfo = extInfo;
            // TRPOS işlem iptal POST URL
            string cancelURL = _config["TrPosAPI:CancelURL"];
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await
                client.PostAsJsonAsync<CancelRequestModel>(cancelURL, requestModel);
                if (response.IsSuccessStatusCode)
                {
                    var cancelResponse =
                    JsonConvert.DeserializeObject<CancelResponseModel>(await
                    response.Content.ReadAsStringAsync()); // Cancel Requestten alınan JSON cevap
                    if (cancelResponse != null)
                    {

                        if (cancelResponse.ResultCode == "0000" && hash == cancelResponse.Hash)
                        {
                            TempData["Message"] = "İptal işlemi başarıyla tamamlandı.";
                            return RedirectToAction("Details", new { id = model.orderId });
                        }
                        else
                        {
                            TempData["Message"] = cancelResponse.ResultDetail;
                        }
                    }
                    else
                    {
                        TempData["Message"] = cancelResponse.ResultDetail;
                    }
                }
                else
                {
                    TempData["Message"] = "Hata: Cevap alınamadı.";
                }
            }
            return RedirectToAction("Details", new { id = model.orderId });
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
    }
}
