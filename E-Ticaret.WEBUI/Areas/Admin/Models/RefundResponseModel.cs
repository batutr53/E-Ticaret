using Azure;
using System.Numerics;

namespace E_Ticaret.WEBUI.Areas.Admin.Models
{
    public class RefundResponseModel
    {
        public string? TxnNo { get; set; }
        public string? RefundTxnNo { get; set; }
        public string? Oid { get; set; }
        public string? ResultCode { get; set; }
        public string? ResultDetail { get; set; }
        public double? Amount { get; set; }
        public string? ExtInfo { get; set; }
        public string? Hash { get; set; }
    }
}
