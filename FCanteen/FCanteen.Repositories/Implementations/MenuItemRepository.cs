using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Repositories.Implementations
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly FCanteenContext _context;

        public MenuItemRepository(FCanteenContext context)
        {
            _context = context;
        }

        public async Task<List<MenuItem>> GetAllAsync()
        {
            return await _context.MenuItems.AsNoTracking().ToListAsync();
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            return await _context.MenuItems.FindAsync(id);
        }

        public async Task UpdateAsync(MenuItem item)
        {
            _context.MenuItems.Update(item);
            await _context.SaveChangesAsync();
        }
    }
}