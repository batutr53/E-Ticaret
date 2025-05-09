using E_Ticaret.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return View();

            return View(contact);
        }

    }
}
