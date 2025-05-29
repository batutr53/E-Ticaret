namespace E_Ticaret.Core.Entities
{
    public class FooterLink
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string IconClass { get; set; }
        public int OrderNo { get; set; }
        public bool IsActive { get; set; }
        public FooterSection? Section { get; set; }
    }

}
