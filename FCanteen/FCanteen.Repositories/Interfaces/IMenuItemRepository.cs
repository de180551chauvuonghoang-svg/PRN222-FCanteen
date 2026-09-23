using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces
{
    public interface IMenuItemRepository
    {
        Task<List<MenuItem>> GetAllAsync();
        Task<MenuItem?> GetByIdAsync(int id);
        Task UpdateAsync(MenuItem item);
    }
}