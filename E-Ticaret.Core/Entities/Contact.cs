using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.Entities
{
    public class Contact : IEntity
    {
        public int Id { get; set; }
        public string NameSurname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
