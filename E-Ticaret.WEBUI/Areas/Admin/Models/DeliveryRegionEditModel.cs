using System.ComponentModel.DataAnnotations;
using E_Ticaret.Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace E_Ticaret.WEBUI.Areas.Admin.Models;

public class DeliveryRegionEditModel
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required] public string Fee { get; set; } = "0";
    public bool IsActive { get; set; } = true;
    public int[] NeighborhoodIds { get; set; } = Array.Empty<int>();
    [ValidateNever] public List<AddressProvince> Provinces { get; set; } = new();
    [ValidateNever] public List<NeighborhoodSelection> SelectedNeighborhoods { get; set; } = new();
}

public sealed record NeighborhoodSelection(int Id, string Label, int ProvinceId, int DistrictId);
