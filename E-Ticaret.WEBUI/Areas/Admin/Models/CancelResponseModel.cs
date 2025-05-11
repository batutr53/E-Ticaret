using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Areas.Admin.Models
{
    public class CancelResponseModel
    {
        public string? TxnNo { get; set; }
        public string? CancelTxnNo { get; set; }
        public string? Oid { get; set; }
        public string? ResultCode { get; set; }
        public string? ExtInfo { get; set; }
        public string? ResultDetail { get; set; }
        public string? Hash { get; set; }
    }
}