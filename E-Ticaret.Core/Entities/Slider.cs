namespace E_Ticaret.Core.Entities
{
    public enum SliderDisplayType
    {
        Desktop = 0,
        Mobile = 1
    }

    public class Slider : IEntity
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public bool IsActive { get; set; }
        public string? Link { get; set; }
        public SliderDisplayType DisplayType { get; set; } = SliderDisplayType.Desktop;
        public int OrderNo { get; set; }
    }
}
