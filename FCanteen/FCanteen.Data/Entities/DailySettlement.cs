using System;

namespace FCanteen.Data.Entities
{
    public class DailySettlement
    {
        public int DailySettlementId { get; set; }
        public DateTime SettlementDate { get; set; }
        public string BranchCode { get; set; } = string.Empty; // CS1, CS2, CS3
        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public long CalculationTimeMs { get; set; }            // Thoi gian tinh toan (ms)
    }
}