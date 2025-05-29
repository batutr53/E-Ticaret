namespace E_Ticaret.Core.Entities
{
    public class FooterContact
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public string Type { get; set; } // phone, mail, whatsapp
        public string IconClass { get; set; }
        public int OrderNo { get; set; }
        public bool IsActive { get; set; }
    }

}
