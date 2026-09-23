using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Discounts
{
    public class TeacherStaffDiscountPolicy : IDiscountPolicy
    {
        public string PolicyName => "Teacher & Staff 15% Discount";
        public int Priority => 2;

        public decimal CalculateDiscount(OrderTicket ticket, Staff? staff, out string reason)
        {
            // Ap dung khi nguoi ban/nguoi mua co role la Staff/Teacher
            if (staff != null && (staff.Role == "Teacher" || staff.Role == "Staff" || staff.Role == "Manager"))
            {
                decimal discount = ticket.TotalAmount * 0.15m;
                reason = $"Giam 15% cho Can bo/Giang vien ({staff.FullName})";
                return discount;
            }
            reason = string.Empty;
            return 0;
        }
    }
}