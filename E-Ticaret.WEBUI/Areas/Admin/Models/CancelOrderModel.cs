using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Ticaret.WEBUI.Areas.Admin.Models
{
    public class CancelOrderModel
    {
        public int orderId { get; set; }
        public string txnNo { get; set; }
        public string oid { get; set; }
        public string? userIP { get; set; }
    }
}