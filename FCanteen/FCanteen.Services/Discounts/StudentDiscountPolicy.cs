using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Discounts
{
    public class StudentDiscountPolicy : IDiscountPolicy
    {
        public string PolicyName => "Student 10% Discount";
        public int Priority => 1;

        public decimal CalculateDiscount(OrderTicket ticket, Staff? staff, out string reason)
        {
            // Ap dung khi khach la sinh vien (mac dinh hoac theo note)
            decimal discount = ticket.TotalAmount * 0.10m;
            reason = "Giam 10% danh cho Sinh vien FPT";
            return discount;
        }
    }
}