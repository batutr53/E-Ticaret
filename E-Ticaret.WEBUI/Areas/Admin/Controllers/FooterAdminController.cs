using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using E_Ticaret.WEBUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FooterAdminController : Controller
    {
        private readonly DatabaseContext _context;
        public FooterAdminController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new FooterAdminViewModel
            {
                Sections = await _context.FooterSections.Include(x => x.Links).OrderBy(x => x.OrderNo).ToListAsync(),
                Contacts = await _context.FooterContacts.OrderBy(x => x.OrderNo).ToListAsync(),
                MobileMenus = await _context.FooterMobileMenus.OrderBy(x => x.OrderNo).ToListAsync(),
            };
            return View(model);
        }

        // === FooterSection CRUD ===
        [HttpPost]
        public async Task<IActionResult> AddSection(FooterSection section)
        {
            if (ModelState.IsValid)
            {
                _context.FooterSections.Add(section);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditSection(FooterSection section)
        {
            if (ModelState.IsValid)
            {
                _context.FooterSections.Update(section);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _context.FooterSections.FindAsync(id);
            if (section != null)
            {
                _context.FooterSections.Remove(section);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // === FooterLink CRUD ===
        [HttpPost]
        public async Task<IActionResult> AddLink(FooterLink link)
        {
            if (ModelState.IsValid)
            {
                _context.FooterLinks.Add(link);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditLink(FooterLink link)
        {
            if (ModelState.IsValid)
            {
                _context.FooterLinks.Update(link);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLink(int id)
        {
            var link = await _context.FooterLinks.FindAsync(id);
            if (link != null)
            {
                _context.FooterLinks.Remove(link);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // === FooterContact CRUD ===
        [HttpPost]
        public async Task<IActionResult> AddContact(FooterContact contact)
        {
            if (ModelState.IsValid)
            {
                _context.FooterContacts.Add(contact);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditContact(FooterContact contact)
        {
            if (ModelState.IsValid)
            {
                _context.FooterContacts.Update(contact);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var contact = await _context.FooterContacts.FindAsync(id);
            if (contact != null)
            {
                _context.FooterContacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // === FooterMobileMenu CRUD ===
        [HttpPost]
        public async Task<IActionResult> AddMobileMenu(FooterMobileMenu mobileMenu)
        {
            if (ModelState.IsValid)
            {
                _context.FooterMobileMenus.Add(mobileMenu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EditMobileMenu(FooterMobileMenu mobileMenu)
        {
            if (ModelState.IsValid)
            {
                _context.FooterMobileMenus.Update(mobileMenu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMobileMenu(int id)
        {
            var menu = await _context.FooterMobileMenus.FindAsync(id);
            if (menu != null)
            {
                _context.FooterMobileMenus.Remove(menu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
