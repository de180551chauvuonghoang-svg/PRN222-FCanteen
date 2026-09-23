using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FCanteenContext _context;

        public OrderRepository(FCanteenContext context)
        {
            _context = context;
        }

        public async Task<OrderTicket?> GetByIdAsync(int id)
        {
            return await _context.OrderTickets
                .Include(t => t.TicketLines)
                .ThenInclude(l => l.MenuItem)
                .FirstOrDefaultAsync(t => t.OrderTicketId == id);
        }

        public async Task<List<OrderTicket>> GetRecentTicketsAsync(int count = 10)
        {
            return await _context.OrderTickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .Include(t => t.TicketLines)
                .ThenInclude(l => l.MenuItem)
                .ToListAsync();
        }

        public async Task<OrderTicket> CreateOrderAsync(OrderTicket ticket)
        {
            _context.OrderTickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task AddDiscountLogAsync(DiscountPolicyLog log)
        {
            _context.DiscountPolicyLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}