using System.ComponentModel.DataAnnotations;

namespace E_Ticaret.Core.Entities
{
    public class MobileBanner : IEntity
    {
        public int Id { get; set; }
        public string? Image { get; set; }

        [MaxLength(500)]
        public string? Link { get; set; }

        public bool IsActive { get; set; }
        public int OrderNo { get; set; }
    }
}
