using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Areas.Admin.Models
{
    public class CancelRequestModel
    {
        public int orderId { get; set; }
        public string? TxnNo { get; set; }
        public string? UserIP { get; set; }
        public string? Oid { get; set; }
        public string? PublicKey { get; set; }
        public string? ApiKey { get; set; }
        public string? Rnd { get; set; }
        public string? Hash { get; set; }
        public string? ExtInfo { get; set; }
    }
}