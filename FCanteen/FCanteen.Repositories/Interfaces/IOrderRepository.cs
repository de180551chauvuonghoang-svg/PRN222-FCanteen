using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<OrderTicket?> GetByIdAsync(int id);
        Task<List<OrderTicket>> GetRecentTicketsAsync(int count = 10);
        Task<OrderTicket> CreateOrderAsync(OrderTicket ticket);
        Task AddDiscountLogAsync(DiscountPolicyLog log);
    }
}