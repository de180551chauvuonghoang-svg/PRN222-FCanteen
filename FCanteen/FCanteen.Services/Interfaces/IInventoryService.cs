using System.Threading.Tasks;

namespace FCanteen.Services.Interfaces
{
    public interface IInventoryService
    {
        Task CheckAndDeductStockAsync(int ingredientId, int amount);
    }
}