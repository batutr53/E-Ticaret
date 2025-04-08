using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.Entities
{
    public class Address : IEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string OpenAddress { get; set; }
        public bool IsActive { get; set; }
        public bool IsBillingAddress { get; set; }
        public bool IsDeliveryAddress { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? AddressGuid { get; set; } = Guid.NewGuid();
        public int? UserId { get; set; }
    }
}
