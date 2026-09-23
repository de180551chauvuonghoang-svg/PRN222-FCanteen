using FCanteen.Data.Entities;

namespace FCanteen.Services.Interfaces
{
    public interface IDiscountPolicy
    {
        string PolicyName { get; }
        int Priority { get; } // Thu tu uu tien ap dung (so nho chay truoc)
        decimal CalculateDiscount(OrderTicket ticket, Staff? staff, out string reason);
    }
}