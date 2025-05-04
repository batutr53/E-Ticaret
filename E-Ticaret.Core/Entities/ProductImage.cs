using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.Entities
{
    public class ProductImage : IEntity
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!; 
        public bool IsDefault { get; set; } = false;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Order { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
