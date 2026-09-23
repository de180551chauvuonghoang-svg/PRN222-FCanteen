using System;
using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Discounts
{
    public class HappyHourDiscountPolicy : IDiscountPolicy
    {
        public string PolicyName => "Happy Hour 20% Discount After 19:00";
        public int Priority => 4;

        public decimal CalculateDiscount(OrderTicket ticket, Staff? staff, out string reason)
        {
            if (ticket.CreatedAt.Hour >= 19 || DateTime.Now.Hour >= 19)
            {
                decimal discount = ticket.TotalAmount * 0.20m;
                reason = "Giam 20% Gio vang Happy Hour sau 19h";
                return discount;
            }

            reason = string.Empty;
            return 0;
        }
    }
}