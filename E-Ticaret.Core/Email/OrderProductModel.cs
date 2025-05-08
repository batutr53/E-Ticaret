namespace E_Ticaret.Core.Email
{
    public class OrderProductModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
