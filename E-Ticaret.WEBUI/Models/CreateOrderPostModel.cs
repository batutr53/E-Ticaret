namespace E_Ticaret.WEBUI.Models
{
    public class CreateOrderPostModel
    {
        public string? SenderFirstName { get; set; }
        public string? SenderLastName { get; set; }
        public string? SenderPhone { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? AddresLine { get; set; }
        public string? City { get; set; }

        public string? CustomerId { get; set; }
        public string? Oid { get; set; }
        public byte Installment { get; set; }
        public decimal DeliveryFree { get; set; }



        public string? CardName { get; set; }
        public string? CardNumber { get; set; }
        public string? CardExpireMonth { get; set; }
        public string? CardExpireYear { get; set; }
        public string? CardCvc { get; set; }

        public CartViewModel Cart { get; set; }
    }
}
