namespace E_Ticaret.Core.DTO
{
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<int> CategoryIds { get; set; } // checkbox’lardan gelen kategori id’leri
    }
}
