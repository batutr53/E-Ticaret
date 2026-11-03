using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.DTO
{
    public class CreateOrderDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? AddressLine { get; set; }
        public string? CustomerId { get; set; }
        public string? Oid { get; set; }
        public string? SenderFirstName { get; set; }
        public string? SenderLastName { get; set; }
        public string? SenderPhone { get; set; }
        public string? SenderEmail { get; set; }
        public string? Description { get; set; }
        public string? CardMessage { get; set; }
        // Legacy client field; delivery pricing is resolved exclusively from NeighborhoodId.
        public int? DeliveryRegionId { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public string? QuotedDeliveryFee { get; set; }
        public decimal DeliveryFree { get; set; }
        public DateTime? DeliveryDate { get; set; } = DateTime.UtcNow;
        public int? DeliveryTimeRangeId { get; set; }
        public string? FirstLastName
        {
            get => $"{FirstName} {LastName}".Trim();
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                    {
                        FirstName = parts[0];
                        LastName = null;
                    }
                    else
                    {
                        FirstName = string.Join(' ', parts.Take(parts.Length - 1));
                        LastName = parts.Last();
                    }
                }
                else
                {
                    FirstName = null;
                    LastName = null;
                }
            }
        }
        public string? SenderFirstLastName
        {
            get => $"{SenderFirstName} {SenderLastName}".Trim();
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 1)
                    {
                        SenderFirstName = parts[0];
                        SenderLastName = null;
                    }
                    else
                    {
                        SenderFirstName = string.Join(' ', parts.Take(parts.Length - 1));
                        SenderLastName = parts.Last();
                    }
                }
                else
                {
                    SenderFirstName = null;
                    SenderLastName = null;
                }
            }
        }
        public CartDto? Cart { get; set; }
    }
}
