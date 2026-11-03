using System.ComponentModel.DataAnnotations;

namespace E_Ticaret.Core.Entities;

public class AddressProvince
{
    public int Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = "";
}

public class AddressDistrict
{
    public int Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = "";
    public int ProvinceId { get; set; }
    public AddressProvince Province { get; set; } = null!;
}

public class AddressNeighborhood
{
    public int Id { get; set; }
    [MaxLength(160)] public string Name { get; set; } = "";
    [MaxLength(120)] public string? LocalityName { get; set; }
    public int DistrictId { get; set; }
    public AddressDistrict District { get; set; } = null!;
    public int? DeliveryRegionId { get; set; }
    public DeliveryRegion? DeliveryRegion { get; set; }
    public string DisplayName => string.IsNullOrWhiteSpace(LocalityName) ? Name : $"{Name} ({LocalityName})";
}
