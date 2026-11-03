using E_Ticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.WEBUI.Controllers;

[Route("DeliveryAddress")]
public class DeliveryAddressController(DatabaseContext db) : Controller
{
    [HttpGet("Districts")]
    public async Task<IActionResult> Districts(int provinceId, CancellationToken cancellationToken)
        => Json(await db.AddressDistricts.AsNoTracking().Where(x => x.ProvinceId == provinceId &&
                db.AddressNeighborhoods.Any(n => n.DistrictId == x.Id && n.DeliveryRegion != null && n.DeliveryRegion.IsActive))
            .OrderBy(x => x.Name).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken));

    [HttpGet("Neighborhoods")]
    public async Task<IActionResult> Neighborhoods(int districtId, CancellationToken cancellationToken)
        => Json(await db.AddressNeighborhoods.AsNoTracking()
            .Where(x => x.DistrictId == districtId && x.DeliveryRegion != null && x.DeliveryRegion.IsActive)
            .OrderBy(x => x.Name).ThenBy(x => x.LocalityName)
            .Select(x => new { x.Id, Name = x.LocalityName == null ? x.Name : x.Name + " (" + x.LocalityName + ")", Fee = x.DeliveryRegion!.Fee })
            .ToListAsync(cancellationToken));
}
