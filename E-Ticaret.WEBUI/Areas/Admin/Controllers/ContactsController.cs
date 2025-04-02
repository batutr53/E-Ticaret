using E_Ticaret.Data;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactsController : Controller
    {
        private readonly DatabaseContext _context;

        public ContactsController(DatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(_context.Contacts);
        }
    }
}
