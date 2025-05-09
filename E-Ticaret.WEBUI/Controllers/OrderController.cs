using Azure.Core;
using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Email;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.Service.Concrete;
using E_Ticaret.WEBUI.ExtensionMethods;
using E_Ticaret.WEBUI.Helpers;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace E_Ticaret.WEBUI.Controllers
{
    [Route("Order")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;
        private readonly ICartService _cartService;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly IRazorViewToStringRenderer _razorRenderer;
        public OrderController(IOrderService orderService, IConfiguration config, DatabaseContext context, ICartService cartService, IEmailService emailService, IOptions<EmailSettings> emailSettings, IRazorViewToStringRenderer razorRenderer)
        {
            _orderService = orderService;
            _config = config;
            _context = context;
            _cartService = cartService;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _razorRenderer = razorRenderer;
        }
        [HttpGet("GetOrder")]
        public async Task<IActionResult> GetOrder()
        {
            return View();
        }

        [HttpPost("GetOrder")]
        public async Task<IActionResult> GetOrder(string oid)
        {
            var result = await _orderService.GetOrderByOId(oid!);
            if (result == null) { ViewBag.Error = "Böyle bir sipariş bulunamadı."; return View(); }
            ;
            return View(result);
        }
        public async Task<IActionResult> Index()
        {
            var deliveryTimes = await _context.DeliveryTimeRanges
      .Where(x => x.IsActive)
      .OrderBy(x => x.StartTime)
      .ToListAsync();

            ViewBag.DeliveryTimeRanges = deliveryTimes;
            return View();
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(CreateOrderDTO model)
        {
            if (model.DeliveryDate.HasValue)
            {
                model.DeliveryDate = DateTime.SpecifyKind(model.DeliveryDate.Value, DateTimeKind.Utc);
            }
            var userCustomerId = Request.Cookies["customerId"];
            model.CustomerId = userCustomerId;
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == userCustomerId);
            model.Oid = "SP" + DateTime.UtcNow.Year + DateTime.UtcNow.Month.ToString("D2") + DateTime.UtcNow.Day.ToString("D2") + DateTime.UtcNow.Minute.ToString("D2") + Request.Cookies["customerId"];
            var result = await _orderService.CreateOrder(model);
            var paymentResult = await Payment(model, cart, model.Oid);
            return Content(paymentResult.Content, "text/html");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<ContentResult> Payment(CreateOrderDTO input, Cart cart, string OrderId)
        {
            string publicKey = _config["TrPosAPI:PublicKey"];
            string apiKey = _config["TrPosAPI:APIKey"];
            string secretKey = _config["TrPosAPI:SecretKey"];
            // Altyapı sistemini sağlayan entegratörün kimlik numarası.
            string integratorId = "";
            // Her işlemde benzersiz gönderilmesi gereken RND
            string rnd = DateTime.Now.Ticks.ToString();
            // İşlem güvenlik şekli (Ödeme Yöntemi)
            byte storeType = 1; // 1: 3D Secure, 2: None Secure
                                // İşlem tipi
            byte txnType = 1; // 1: Satış
                              // dışarda
            string cardHolder = input.CardName; // Kart sahibi adı
            string cardNumber = input.CardNumber; // Kart
            string expMonth = input.CardExpireMonth; // Kart Son Kullanım

            string expYear = input.CardExpireYear; // Kart Son Kullanım Tarihi – YIL
            string cv2 = input.CardCvc; // Kart Güvenlik Numarası
                                        // Benzersiz sipariş numarası (Örnek olarak verilmiştir. Kendi sipariş numaranızı

            string oid = OrderId;
            // İşlem tutarı
            string amount = cart.CalculateTotal().ToString("N2", new System.Globalization.CultureInfo("tr-TR"));
            byte installment = input.Installment; // Örnek: 0,2,3,4,5,6,7,8,9,10,11,12
                                                  // Para birimi
            byte currency = 1; // 1:TL, 2:USD, 3:EUR

            // Satış işlemine ait cevapların döneceği URL
            string returnUrl = Request.GetBaseUrl(includePort: true) + "/Order/PaymentResponse";
            string userIp = "1.1.1.1";
            //------------
            if (string.IsNullOrEmpty(userIp))
            {
                userIp = HttpContext.GetServerVariable("REMOTE_ADDR");
            }

            string birlestir = string.Concat(apiKey.ToUpper(), publicKey, rnd, oid, amount, currency);
            HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(birlestir));
            string hash = Convert.ToBase64String(b);
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
            { "PublicKey", publicKey },
            { "ApiKey", apiKey.ToUpper() },
            { "Rnd", rnd },
            { "Hash", hash },
            { "StoreType", storeType.ToString() },
            { "TxnType", txnType.ToString() },
            { "CardHolder", cardHolder },
            { "CardNumber", cardNumber },
            { "ExpMonth",expMonth },
            { "ExpYear", expYear },
            { "Cv2", cv2 },
            { "Oid", oid.ToString() },
            { "Amount", amount },
            { "Installment", installment.ToString() },
            { "Currency", currency.ToString() },
            { "ReturnUrl", returnUrl },
            { "UserIP", userIp },
            { "IntegratorId", integratorId }
            };
            // TRPOS Satış İşlemi POST URL
            string paymentURL = _config["TrPosAPI:PaymentURL"];
            try
            {
                using (HttpClient client = new())
                {
                    var formContent = new FormUrlEncodedContent(formData);
                    HttpResponseMessage response = await client.PostAsync(paymentURL, formContent);
                    if (response.IsSuccessStatusCode)
                    {
                        // Ekrana yazdırılması gereken TRPOS tarafından dönen HTML
                        var htmlContent = await response.Content.ReadAsStringAsync();
                        return new ContentResult
                        {
                            Content = htmlContent,
                            ContentType = "text/html",
                            StatusCode = 200
                        };
                    }
                    else
                    {
                        Console.WriteLine("HATA: " + response.StatusCode);
                        return new ContentResult
                        {
                            Content = "HATA: " + response.StatusCode,
                            ContentType = "text/html",
                            StatusCode = 400
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new ContentResult
                {
                    Content = $"Exception: {ex.Message}",
                    ContentType = "text/html",
                    StatusCode = 500
                };
            }
        }

        [HttpPost("PaymentResponse")]
        public async Task<IActionResult> PaymentResponse([FromForm] PaymentResponseDTO model)
        {
            var order = await _orderService.GetOrderById(model.Oid);
            if (model.ResultCode == "0000")
            {
                order.OrderStatus = OrderStatus.Approved;
                order.TxnNo = model.TxnNo;
                order.Oid = model.Oid;

                var customerId = model.Oid.Substring(12);
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CustomerId == model.Oid.Substring(12));

                var products = (order?.OrderItems ?? new List<OrderItem>())
        .Select(x => new OrderProductModel
        {
            Name = x.ProductName,
            ImageUrl = ImageHelper.ToFullImageUrl(x.ProductImage ?? "no-image.png"),
            Quantity = x.Quantity,
            Price = x.Price
        }).ToList();

                _cartService.Remove(cart);
                await _orderService.UpdateOrder(order);


                NotifyPaymentSuccess(
             order.SenderEmail,
             order.SenderFirstName + " " + order.SenderLastName,
             order.Oid,
             order.OrderDate,
             order.DeliveryDate.Value,
             order.DeliveryTimeRange.RangeText,
             products,
             senderFullName: order.SenderFirstName + " " + order.SenderLastName,
             senderPhone: order.SenderPhone,
             senderEmail: order.SenderEmail,
             receiverFullName: order.FirstName + " " + order.LastName,
             receiverPhone: order.Phone,
             receiverAddress: order.City + " - " + order.AddresLine,
             subTotal: order.SubTotal,
             deliveryFee: order.DeliveryFree,
             total: order.GetTotal()
         );


                return View(model);
            }
            order.OrderStatus = OrderStatus.PaymentFailed;
            await _orderService.UpdateOrder(order);
            return View(model);

        }

        [HttpPost]
        [Route("order/notify-payment")]
        public IActionResult NotifyPaymentSuccess(
    string customerEmail,
    string customerName,
    string orderNo,
    DateTime orderDate,
    DateTime deliveryDate,
    string DeliveryTimeRange,
    [FromBody] List<OrderProductModel> products,
    string senderFullName,
    string senderPhone,
    string senderEmail,
    string receiverFullName,
    string receiverPhone,
    string receiverAddress,
    decimal subTotal,
    decimal deliveryFee,
    decimal total
)
        {
            var emailModel = new OrderEmailModel
            {
                CustomerName = customerName,
                OrderNo = orderNo,
                OrderDate = orderDate,
                DeliveryDate = deliveryDate,
                DeliveryTimeRange = DeliveryTimeRange,
                Products = products,
                SenderFullName = senderFullName,
                SenderPhone = senderPhone,
                SenderEmail = senderEmail,
                ReceiverFullName = receiverFullName,
                ReceiverPhone = receiverPhone,
                ReceiverAddress = receiverAddress,
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                Total = total
            };


            var bodyHtml = _razorRenderer.RenderViewToStringAsync("Emails/OrderConfirmation", emailModel).Result;

            _emailService.SendEmailAsync(customerEmail, "Siparişiniz Alındı - Detay Çiçekcilik", bodyHtml);
            _emailService.SendEmailAsync(_emailSettings.AdminEmail, $"Yeni Sipariş - Detay Çiçekcilik", bodyHtml);

            return Ok();
        }

    }
}
