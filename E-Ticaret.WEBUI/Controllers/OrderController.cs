using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Email;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.Service.Concrete;
using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Models;
using E_Ticaret.WEBUI.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net;

namespace E_Ticaret.WEBUI.Controllers;

[Route("Order")]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly DatabaseContext _context;
    private readonly ICartService _cartService;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly IRazorViewToStringRenderer _razorRenderer;
    private readonly TelegramHelper _telegramHelper;
    private readonly IPayTrService _payTrService;
    private readonly PayTrOptions _payTrOptions;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IOrderService orderService,
        DatabaseContext context,
        ICartService cartService,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        IRazorViewToStringRenderer razorRenderer,
        TelegramHelper telegramHelper,
        IPayTrService payTrService,
        IOptions<PayTrOptions> payTrOptions,
        ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _context = context;
        _cartService = cartService;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _razorRenderer = razorRenderer;
        _telegramHelper = telegramHelper;
        _payTrService = payTrService;
        _payTrOptions = payTrOptions.Value;
        _logger = logger;
    }

    [HttpGet("GetOrder")]
    public IActionResult GetOrder() => View();

    [HttpPost("GetOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetOrder(string oid)
    {
        var result = await _orderService.GetOrderByOId(oid);
        if (result == null)
        {
            ViewBag.Error = "Böyle bir sipariş bulunamadı.";
            return View();
        }

        return View(result);
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var customerId = Request.Cookies["customerId"];
        if (await _context.Carts.Where(x => x.CustomerId == customerId)
            .SelectMany(x => x.CartItems).AnyAsync(x => !x.Product.IsActive))
        {
            TempData["CartWarning"] = "Sepetinizde artık satışta olmayan bir ürün var. Devam etmek için bu ürünü sepetten çıkarın.";
            return RedirectToAction("Index", "Cart");
        }
        ViewBag.DeliveryTimeRanges = await _context.DeliveryTimeRanges
            .Where(x => x.IsActive)
            .OrderBy(x => x.StartTime)
            .ToListAsync();
        ViewBag.DeliveryProvinces = await _context.AddressProvinces.AsNoTracking()
            .Where(p => _context.AddressNeighborhoods.Any(n => n.District.ProvinceId == p.Id && n.DeliveryRegion != null && n.DeliveryRegion.IsActive))
            .OrderBy(p => p.Name).ToListAsync();
        return View();
    }

    [HttpPost("CreateOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(CreateOrderDTO model, CancellationToken cancellationToken)
    {
        var validationError = ValidateOrderInput(model);
        if (validationError != null)
        {
            return PaymentResult(false, false, string.Empty, validationError);
        }

        if (model.DeliveryDate.HasValue)
        {
            model.DeliveryDate = DateTime.SpecifyKind(model.DeliveryDate.Value, DateTimeKind.Utc);
        }

        var customerId = Request.Cookies["customerId"];
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return PaymentResult(false, false, string.Empty, "Sepet oturumu bulunamadı. Lütfen sepetinizi yenileyip tekrar deneyin.");
        }

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
        if (cart == null || cart.CartItems.Count == 0)
        {
            return PaymentResult(false, false, string.Empty, "Sepetiniz boş. Ödeme başlatılamadı.");
        }

        if (!await _context.DeliveryTimeRanges.AnyAsync(x => x.Id == model.DeliveryTimeRangeId && x.IsActive, cancellationToken))
            return PaymentResult(false, false, string.Empty, "Seçilen teslimat saati artık uygun değil. Lütfen yeniden seçin.");
        if (model.DeliveryDate!.Value.Date < DateTime.UtcNow.AddHours(3).Date)
            return PaymentResult(false, false, string.Empty, "Geçmiş bir teslimat tarihi seçilemez.");
        model.CustomerId = customerId;
        model.Oid = CreateMerchantOid();
        Order order;
        try { order = await _orderService.CreateOrder(model); }
        catch (InvalidOperationException exception)
        {
            return PaymentResult(false, false, string.Empty, exception.Message);
        }
        if (order.Id == 0)
        {
            return PaymentResult(false, false, model.Oid, "Sipariş oluşturulamadı. Lütfen tekrar deneyin.");
        }

        var siteUrl = GetSiteUrl();
        var successUrl = $"{siteUrl}/Order/PaymentSuccess?merchantOid={Uri.EscapeDataString(order.Oid!)}";
        var failureUrl = $"{siteUrl}/Order/PaymentFailed?merchantOid={Uri.EscapeDataString(order.Oid!)}";
        var tokenResult = await _payTrService.CreatePaymentTokenAsync(
            order,
            GetClientIpAddress(),
            successUrl,
            failureUrl,
            cancellationToken);

        if (!tokenResult.IsSuccess || string.IsNullOrWhiteSpace(tokenResult.Token))
        {
            order.OrderStatus = OrderStatus.PaymentFailed;
            await _orderService.UpdateOrder(order);
            _logger.LogWarning("PayTR token could not be created for order {MerchantOid}: {Reason}", order.Oid, tokenResult.ErrorMessage);
            return PaymentResult(false, false, order.Oid!, tokenResult.ErrorMessage ?? "Ödeme ekranı açılamadı.");
        }

        return View("PayTrPayment", new PayTrPaymentViewModel
        {
            MerchantOid = order.Oid!,
            PaymentUrl = _payTrService.GetPaymentUrl(tokenResult.Token)
        });
    }

    [HttpPost("PayTrCallback")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PayTrCallback(
        [FromForm] PayTrCallbackModel callback,
        CancellationToken cancellationToken)
    {
        if (!_payTrService.ValidateCallback(callback))
        {
            _logger.LogWarning("Invalid PayTR callback hash for order {MerchantOid}", callback.MerchantOid);
            return BadRequest("PAYTR notification failed: bad hash");
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.DeliveryTimeRange)
            .FirstOrDefaultAsync(o => o.Oid == callback.MerchantOid, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("PayTR callback received for unknown order {MerchantOid}", callback.MerchantOid);
            return NotFound("PAYTR notification failed: order not found");
        }

        if (order.OrderStatus != OrderStatus.Pending)
        {
            return Content("OK", "text/plain");
        }

        var expectedAmount = decimal.Round(order.GetTotal() * 100m, 0, MidpointRounding.AwayFromZero)
            .ToString("0", CultureInfo.InvariantCulture);
        if (!string.Equals(callback.PaymentAmount, expectedAmount, StringComparison.Ordinal))
        {
            _logger.LogError(
                "PayTR callback amount mismatch for order {MerchantOid}. Expected {Expected}, received {Received}",
                callback.MerchantOid,
                expectedAmount,
                callback.PaymentAmount);
            return BadRequest("PAYTR notification failed: amount mismatch");
        }

        if (string.Equals(callback.Status, "success", StringComparison.OrdinalIgnoreCase))
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var transitioned = await _context.Orders
                .Where(o => o.Id == order.Id && o.OrderStatus == OrderStatus.Pending)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(o => o.OrderStatus, OrderStatus.Approved),
                    cancellationToken);
            if (transitioned == 0)
            {
                return Content("OK", "text/plain");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == order.CustomerId, cancellationToken);
            if (cart != null)
            {
                _cartService.Remove(cart);
            }

            await _context.SaveChangesAsync(cancellationToken);
            _context.TelegramNotifications.Add(new TelegramNotification { OrderId = order.Id });
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await SendSuccessNotificationsSafelyAsync(order);
        }
        else
        {
            await _context.Orders
                .Where(o => o.Id == order.Id && o.OrderStatus == OrderStatus.Pending)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(o => o.OrderStatus, OrderStatus.PaymentFailed),
                    cancellationToken);
            _logger.LogInformation(
                "PayTR payment failed for order {MerchantOid}. Code: {Code}, reason: {Reason}",
                callback.MerchantOid,
                callback.FailedReasonCode,
                callback.FailedReasonMessage);
        }

        return Content("OK", "text/plain");
    }

    [HttpGet("PaymentSuccess")]
    public async Task<IActionResult> PaymentSuccess(string merchantOid, CancellationToken cancellationToken)
    {
        var status = await _context.Orders
            .Where(o => o.Oid == merchantOid)
            .Select(o => (OrderStatus?)o.OrderStatus)
            .FirstOrDefaultAsync(cancellationToken);

        if (status is OrderStatus.Approved or OrderStatus.Ready or OrderStatus.OnWay or OrderStatus.Completed)
        {
            return PaymentResult(true, false, merchantOid, "Ödemeniz onaylandı. Siparişiniz hazırlanmak üzere alındı.");
        }

        if (status == OrderStatus.PaymentFailed)
        {
            return PaymentResult(false, false, merchantOid, "Ödeme onaylanamadı. Lütfen tekrar deneyin.");
        }

        return PaymentResult(false, true, merchantOid, "Ödeme sonucunuz PayTR tarafından doğrulanıyor. Sipariş durumunuz kısa süre içinde güncellenecek.");
    }

    [HttpGet("PaymentFailed")]
    public IActionResult PaymentFailed(string merchantOid) =>
        PaymentResult(false, false, merchantOid, "Ödeme tamamlanamadı. Kartınızdan tahsilat yapıldıysa sipariş durumu PayTR bildirimi geldikten sonra otomatik güncellenecektir.");

    private IActionResult PaymentResult(bool isSuccess, bool isPending, string merchantOid, string message) =>
        View("PaymentResponse", new PaymentResultViewModel
        {
            IsSuccess = isSuccess,
            IsPending = isPending,
            MerchantOid = merchantOid,
            Message = message
        });

    private async Task SendSuccessNotificationsSafelyAsync(Order order)
    {
        try
        {
            var products = order.OrderItems.Select(x => new OrderProductModel
            {
                Name = x.ProductName,
                ImageUrl = ImageHelper.ToFullImageUrl(x.ProductImage ?? "no-image.png"),
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList();
            var emailModel = new OrderEmailModel
            {
                CustomerName = $"{order.SenderFirstName} {order.SenderLastName}".Trim(),
                OrderNo = order.Oid,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate ?? order.OrderDate,
                DeliveryTimeRange = order.DeliveryTimeRange?.RangeText ?? "-",
                Products = products,
                SenderFullName = $"{order.SenderFirstName} {order.SenderLastName}".Trim(),
                SenderPhone = order.SenderPhone,
                SenderEmail = order.SenderEmail,
                ReceiverFullName = $"{order.FirstName} {order.LastName}".Trim(),
                ReceiverPhone = order.Phone,
                ReceiverAddress = $"{order.DeliveryLocation} - {order.AddresLine}",
                SubTotal = order.SubTotal,
                DeliveryFee = order.DeliveryFree,
                Total = order.GetTotal()
            };

            emailModel.CardMessage = order.CardMessage;
            emailModel.DeliveryInstructions = order.Description;
            emailModel.DeliveryRegionName = order.DeliveryRegionName;
            var bodyHtml = await _razorRenderer.RenderViewToStringAsync("Emails/OrderConfirmation", emailModel);
            if (!string.IsNullOrWhiteSpace(order.SenderEmail))
            {
                await SendEmailSafelyAsync(order.SenderEmail, "Siparişiniz Alındı - Detay Çiçekcilik", bodyHtml, order.Oid);
            }
            await SendEmailSafelyAsync(_emailSettings.AdminEmail, "Yeni Sipariş - Detay Çiçekcilik", bodyHtml, order.Oid);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Payment succeeded but notification delivery failed for order {MerchantOid}", order.Oid);
        }
    }

    private async Task SendEmailSafelyAsync(string email, string subject, string body, string? oid)
    {
        try { await _emailService.SendEmailAsync(email, subject, body); }
        catch (Exception) { _logger.LogWarning("Sipariş {MerchantOid} e-posta bildirimi gönderilemedi.", oid); }
    }

    private string GetSiteUrl()
    {
        if (!string.IsNullOrWhiteSpace(_payTrOptions.SiteUrl))
        {
            return _payTrOptions.SiteUrl.TrimEnd('/');
        }

        return $"{Request.Scheme}://{Request.Host}";
    }

    private string GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();
        if (IPAddress.TryParse(forwardedFor, out var forwardedIp))
        {
            return NormalizeIp(forwardedIp) ?? "127.0.0.1";
        }

        return NormalizeIp(HttpContext.Connection.RemoteIpAddress) ?? "127.0.0.1";
    }

    private static string? NormalizeIp(IPAddress? address) =>
        address?.IsIPv4MappedToIPv6 == true ? address.MapToIPv4().ToString() : address?.ToString();

    private static string CreateMerchantOid() =>
        $"SP{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..40];

    private static string? ValidateOrderInput(CreateOrderDTO model)
    {
        if (string.IsNullOrWhiteSpace(model.SenderFirstLastName) || model.SenderFirstLastName.Length > 60)
            return "Gönderici adı zorunludur ve en fazla 60 karakter olabilir.";
        if (string.IsNullOrWhiteSpace(model.SenderEmail) ||
            model.SenderEmail.Length > 100 ||
            model.SenderEmail.Any(character => character > 127) ||
            !new EmailAddressAttribute().IsValid(model.SenderEmail))
            return "Geçerli bir gönderici e-posta adresi girin.";
        if (string.IsNullOrWhiteSpace(model.SenderPhone) || model.SenderPhone.Length > 20)
            return "Geçerli bir gönderici telefonu girin.";
        if (string.IsNullOrWhiteSpace(model.FirstLastName))
            return "Alıcı adı zorunludur.";
        if (string.IsNullOrWhiteSpace(model.AddressLine) || model.AddressLine.Length > 400)
            return "Alıcı adresi zorunludur ve en fazla 400 karakter olabilir.";
        if (string.IsNullOrWhiteSpace(model.Phone) || model.Phone.Length > 20)
            return "Geçerli bir alıcı telefonu girin.";
        if (!model.DeliveryDate.HasValue || !model.DeliveryTimeRangeId.HasValue)
            return "Teslimat tarihi ve saat aralığını seçin.";
        if (model.CardMessage?.Length > 500 || model.Description?.Length > 500)
            return "Kart mesajı ve teslimat talebi en fazla 500 karakter olabilir.";

        return null;
    }
}
