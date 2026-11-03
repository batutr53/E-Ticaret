using System.ComponentModel.DataAnnotations;

namespace E_Ticaret.Core.Entities;

public class DeliveryRegion
{
    public int Id { get; set; }
    [Required, StringLength(120)]
    public string Name { get; set; } = "";
    [Range(typeof(decimal), "0", "1000000")]
    public decimal Fee { get; set; }
    public bool IsActive { get; set; } = true;
    public List<AddressNeighborhood> Neighborhoods { get; set; } = new();
}
