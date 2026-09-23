using System.Linq;
using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Discounts
{
    public class ComboDiscountPolicy : IDiscountPolicy
    {
        public string PolicyName => "Combo Meal + Drink Discount 5,000 VND";
        public int Priority => 3;

        public decimal CalculateDiscount(OrderTicket ticket, Staff? staff, out string reason)
        {
            // Kiem tra co ca mon an (com, pho, bun, mi, xoi...) va do uong (nuoc, tra, sinh to, che)
            bool hasMainDish = ticket.TicketLines.Any(l => 
                l.MenuItem != null && (l.MenuItem.Unit == "phan" || l.MenuItem.Unit == "to" || l.MenuItem.Unit == "o"));
            bool hasDrink = ticket.TicketLines.Any(l => 
                l.MenuItem != null && l.MenuItem.Unit == "ly");

            if (hasMainDish && hasDrink)
            {
                reason = "Giam 5.000 VND combo Mon an chinh kem Do uong";
                return 5000m;
            }

            reason = string.Empty;
            return 0;
        }
    }
}