using E_Ticaret.Core.DTO;
using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using E_Ticaret.Service.Concrete;
using E_Ticaret.WEBUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace E_Ticaret.WEBUI.Controllers
{
    public class OrderController : Controller
    {
       private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly DatabaseContext _context;
        private readonly IConfiguration _config;

        public OrderController(IOrderService orderService, IConfiguration config, ICartService cartService, DatabaseContext context)
        {
            _orderService = orderService;
            _config = config;
            _cartService = cartService;
            _context = context;
        }

        public async Task<IActionResult> GetOrder()
        {
            var userCustomerId = Request.Cookies["userCustomerId"];
            var result = await _orderService.GetOrder(userCustomerId!);
            return View(result.ToList());
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(CreateOrderPostModel orderDTO)
        {
            var userCustomerId = Request.Cookies["userCustomerId"];
            orderDTO.CustomerId = userCustomerId!;
            var cart = await _cartService.GetCartById(userCustomerId);
            var paymentResult = await Payment(orderDTO, cart);

            var orderModel = new Order()
            {
                FirstName = orderDTO.FirstName,
                LastName = orderDTO.LastName,
                AddresLine = orderDTO.AddresLine,
                City = orderDTO.City,
                DeliveryFree = orderDTO.DeliveryFree,
                CustomerId = orderDTO.CustomerId,
                OrderDate = DateTime.UtcNow,
            };
            var items = new List<OrderItem>();
            foreach (var item in cart.CartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                var orderItem = new OrderItem
                {
                    ProductId = product!.Id,
                    ProductName = product.Name!,
                    ProductImage = product.Image!,
                    Price = product.Price,
                    Quantity = item.Quantity
                };
                items.Add(orderItem);
                product.Stock -= item.Quantity;
            }
            var SubTotal = items.Sum(i => i.Price * i.Quantity);
            var deliveryFee = 0;
            orderDTO.Oid = Convert.ToString(orderDTO.Oid + RandomNumberGenerator.GetInt32(0, 9999));

            var order = new Order
            {
                Id = Convert.ToInt32(model.Oid),
                OrderItems = items,
                CustomerId = Request.Cookies["customerId"],
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                City = model.City,
                AddresLine = model.AddressLine,
                SubTotal = SubTotal,
                DeliveryFree = deliveryFee
            };


            var result = await _orderService.CreateOrder(orderDTO);
            return RedirectToAction("GetOrder");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<ContentResult> Payment(CreateOrderPostModel input, Cart cart)
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

            string oid = input.Oid;
            // İşlem tutarı
            string amount = "222";
            byte installment = input.Installment; // Örnek: 0,2,3,4,5,6,7,8,9,10,11,12
                                                  // Para birimi
            byte currency = 1; // 1:TL, 2:USD, 3:EUR

            // Satış işlemine ait cevapların döneceği URL
            string returnUrl = "http://localhost:6969/api/Order/PaymentResponse";
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
            { "Oid", oid },
            { "Amount", amount },
            { "Installment", installment.ToString() },
            { "Currency", currency.ToString() },
            { "ReturnUrl", returnUrl },
            { "UserIP", userIp },
            { "IntegratorId", integratorId }
            };
            // TRPOS Satış İşlemi POST URL
            string paymentURL = _config["TrPosAPI:PaymentURL"]; ;
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
            if (model.ResultCode == "0000")
            {
                var userCustomerId = Request.Cookies["userCustomerId"];
                var order = await _orderService.GetOrderById(Convert.ToInt32(model.Oid), userCustomerId);
                order.OrderStatus = OrderStatus.Completed;
                //await _orderService.SaveChangesAsync();
                return Redirect("http://localhost:9696/payment/status?id=" + model.Oid);
            }
            else
            {
                return Redirect("http://localhost:9696/payment/status?message=" + model.ResultCode);
            }
        }

    }
}
