using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Repositories.Implementations
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly FCanteenContext _context;

        public IngredientRepository(FCanteenContext context)
        {
            _context = context;
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            return await _context.Ingredients.AsNoTracking().ToListAsync();
        }

        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _context.Ingredients.FindAsync(id);
        }

        public async Task DeductStockAsync(int ingredientId, int amount)
        {
            var ing = await _context.Ingredients.FindAsync(ingredientId);
            if (ing != null)
            {
                ing.StockQuantity -= amount;
                await _context.SaveChangesAsync();
            }
        }
    }
}