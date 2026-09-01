namespace E_Ticaret.Core.Entities
{
    public static class ContentPageKeys
    {
        public const string About = "About";
        public const string PrivacySecurity = "PrivacySecurity";
        public const string DeliveryWarranty = "DeliveryWarranty";

        public static readonly string[] All = [About, PrivacySecurity, DeliveryWarranty];
    }

    public class ContentPage : IEntity
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public string BodyHtml { get; set; } = string.Empty;
    }
}
