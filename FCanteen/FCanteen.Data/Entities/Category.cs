using System.Collections.Generic;

namespace FCanteen.Data.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty; // Mon chinh, Mon phu, Do uong, Trang mieng
        public string Description { get; set; } = string.Empty;
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}