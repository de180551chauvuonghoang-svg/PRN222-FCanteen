using System.Threading.Tasks;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly IIngredientRepository _ingRepo;

        public InventoryService(IIngredientRepository ingRepo)
        {
            _ingRepo = ingRepo;
        }

        public async Task CheckAndDeductStockAsync(int ingredientId, int amount)
        {
            await _ingRepo.DeductStockAsync(ingredientId, amount);
        }
    }
}