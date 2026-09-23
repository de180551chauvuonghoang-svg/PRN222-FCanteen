using System;

namespace FCanteen.Data.Entities
{
    public class DiscountPolicyLog
    {
        public int Id { get; set; }
        public int OrderTicketId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public string Reason { get; set; } = string.Empty;
    }
}