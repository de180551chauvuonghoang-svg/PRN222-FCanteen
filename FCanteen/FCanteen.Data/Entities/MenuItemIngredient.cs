namespace FCanteen.Data.Entities
{
    public class MenuItemIngredient
    {
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } = null!;

        public decimal Quantity { get; set; } // Dinh luong nguyen lieu cho mon (kg, lit,...)
    }
}