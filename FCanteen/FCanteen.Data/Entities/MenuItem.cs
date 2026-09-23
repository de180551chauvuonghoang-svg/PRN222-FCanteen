using System.Collections.Generic;

namespace FCanteen.Data.Entities
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public string ItemCode { get; set; } = "MON-0001"; // MON-xxxx
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<TicketLine> TicketLines { get; set; } = new List<TicketLine>();
        public ICollection<MenuItemIngredient> MenuItemIngredients { get; set; } = new List<MenuItemIngredient>();
    }
}