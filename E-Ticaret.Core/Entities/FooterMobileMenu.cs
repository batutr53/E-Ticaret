namespace E_Ticaret.Core.Entities
{
    public class FooterMobileMenu
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string IconClass { get; set; }
        public int OrderNo { get; set; }
        public bool IsActive { get; set; }
    }
}
