using E_Ticaret.Core.DTO;
using E_Ticaret.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MainController : Controller
    {
        private readonly IOrderService _orderService;

        public MainController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var summary = await _orderService.GetDashboardSummaryAsync();
            return View(summary);
        }
    }
}
