using System.Text.RegularExpressions;
using E_Ticaret.Core.Entities;
using E_Ticaret.Service.Abstract;
using E_Ticaret.WEBUI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Ticaret.WEBUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SiteSettingsController : Controller
    {
        private static readonly Regex HexColor = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);
        private readonly ISiteSettingService _settings;

        public SiteSettingsController(ISiteSettingService settings)
        {
            _settings = settings;
        }

        public async Task<IActionResult> Index() => View(await _settings.GetAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSetting model, IFormFile? Logo, bool removeLogo = false)
        {
            var current = await _settings.GetAsync();
            model.Id = SiteSetting.SingletonId;
            model.Logo = current.Logo;

            if (!HexColor.IsMatch(model.PrimaryColor ?? string.Empty))
                ModelState.AddModelError(nameof(SiteSetting.PrimaryColor), "Renk #RRGGBB formatında olmalıdır.");
            if (!HexColor.IsMatch(model.AccentColor ?? string.Empty))
                ModelState.AddModelError(nameof(SiteSetting.AccentColor), "Renk #RRGGBB formatında olmalıdır.");
            if (!ModelState.IsValid)
                return View(model);

            if (removeLogo && !string.IsNullOrWhiteSpace(model.Logo))
            {
                FileHelper.FileRemover(model.Logo);
                model.Logo = null;
            }

            if (Logo != null)
            {
                var uploaded = await FileHelper.FileLoaderAsync(Logo);
                if (string.IsNullOrWhiteSpace(uploaded))
                {
                    ModelState.AddModelError(nameof(SiteSetting.Logo), "Geçerli bir görsel yükleyin.");
                    return View(model);
                }
                if (!string.IsNullOrWhiteSpace(model.Logo))
                    FileHelper.FileRemover(model.Logo);
                model.Logo = uploaded;
            }

            await _settings.SaveAsync(model);
            TempData["SiteSettingMessage"] = "Site ayarları kaydedildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
