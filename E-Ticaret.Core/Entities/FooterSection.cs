namespace E_Ticaret.Core.Entities
{
    public class FooterSection
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int OrderNo { get; set; }
        public bool IsActive { get; set; }
        public ICollection<FooterLink>? Links { get; set; }
    }

}
