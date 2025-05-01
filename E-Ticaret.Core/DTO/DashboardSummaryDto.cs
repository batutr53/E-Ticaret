using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Core.DTO
{
    public class DashboardSummaryDto
    {
        public decimal MonthlyEarnings { get; set; }
        public decimal AnnualEarnings { get; set; }
        public int TaskCompletionPercent { get; set; }

        public List<string> Last6MonthsLabels { get; set; } = new();
        public List<decimal> Last6MonthsTotals { get; set; } = new();
    }

}
