using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<Staff?> GetByCodeAsync(string staffCode);
        Task<List<Staff>> GetAllAsync();
    }
}