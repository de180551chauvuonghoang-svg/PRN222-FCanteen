using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Repositories.Implementations
{
    public class StaffRepository : IStaffRepository
    {
        private readonly FCanteenContext _context;

        public StaffRepository(FCanteenContext context)
        {
            _context = context;
        }

        public async Task<Staff?> GetByCodeAsync(string staffCode)
        {
            return await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCode == staffCode);
        }

        public async Task<List<Staff>> GetAllAsync()
        {
            return await _context.Staffs.AsNoTracking().ToListAsync();
        }
    }
}