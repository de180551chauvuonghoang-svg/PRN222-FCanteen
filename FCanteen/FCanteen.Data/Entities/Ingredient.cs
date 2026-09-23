using System.Collections.Generic;

namespace FCanteen.Data.Entities
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;     // kg, lit, goi, qua...
        public int StockQuantity { get; set; }               // Ton kho hien tai
        public int WarningThreshold { get; set; }            // Nguong canh bao sap het
        public decimal CostPrice { get; set; }               // Gia von nguyen lieu

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();
    }
}