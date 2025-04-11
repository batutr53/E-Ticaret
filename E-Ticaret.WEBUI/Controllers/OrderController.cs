using E_Ticaret.Data;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Controllers
{
    public class OrderController : Controller
    {
       private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> GetOrder()
        {
            var userCustomerId = Request.Cookies["userCustomerId"];
            var result = await _orderService.GetOrder(userCustomerId!);
            return View(result.ToList());
        }
    }
}
