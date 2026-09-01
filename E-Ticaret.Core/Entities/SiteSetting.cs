namespace E_Ticaret.Core.Entities
{
    public class SiteSetting : IEntity
    {
        public const int SingletonId = 1;

        public int Id { get; set; } = SingletonId;
        public string? Logo { get; set; }
        public string PrimaryColor { get; set; } = "#888888";
        public string AccentColor { get; set; } = "#236B43";
    }
}
