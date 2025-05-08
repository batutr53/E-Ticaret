using E_Ticaret.Core.Email;

namespace E_Ticaret.WEBUI.Models
{
    public class OrderEmailModel
    {
        public string CustomerName { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryTimeRange { get; set; }
        public string SenderFullName { get; set; }
        public string SenderPhone { get; set; }
        public string SenderEmail { get; set; }

        public string ReceiverFullName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public List<OrderProductModel> Products { get; set; } = new();
    }
}
