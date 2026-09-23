using System;

namespace FCanteen.Data.Entities
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;     // kg, lit, goi, qua...
        public int StockQuantity { get; set; }               // Ton kho hien tai
        public int WarningThreshold { get; set; }            // Nguong canh bao sap het
    }
}