using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data.Entities;

namespace FCanteen.Repositories.Interfaces
{
    public interface IIngredientRepository
    {
        Task<List<Ingredient>> GetAllAsync();
        Task<Ingredient?> GetByIdAsync(int id);
        Task DeductStockAsync(int ingredientId, int amount);
    }
}