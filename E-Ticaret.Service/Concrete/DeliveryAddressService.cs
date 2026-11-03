using E_Ticaret.Core.Entities;
using E_Ticaret.Data;
using Microsoft.EntityFrameworkCore;

namespace E_Ticaret.Service.Concrete;

public class DeliveryAddressService(DatabaseContext db)
{
    public async Task<AddressNeighborhood> ResolveAsync(int? provinceId, int? districtId, int? neighborhoodId, CancellationToken cancellationToken = default)
    {
        if (provinceId is null or <= 0 || districtId is null or <= 0 || neighborhoodId is null or <= 0)
            throw new InvalidOperationException("Lütfen teslimat için il, ilçe ve mahalle seçin.");
        var neighborhood = await db.AddressNeighborhoods.AsNoTracking()
            .Include(x => x.District).ThenInclude(x => x.Province).Include(x => x.DeliveryRegion)
            .SingleOrDefaultAsync(x => x.Id == neighborhoodId && x.DistrictId == districtId && x.District.ProvinceId == provinceId, cancellationToken);
        if (neighborhood?.DeliveryRegion?.IsActive != true)
            throw new InvalidOperationException("Seçtiğiniz mahalleye şu anda teslimat yapılamıyor. Lütfen adres seçiminizi kontrol edin.");
        return neighborhood;
    }
}
