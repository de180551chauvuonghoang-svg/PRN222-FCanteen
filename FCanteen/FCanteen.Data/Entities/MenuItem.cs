using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities
{
   public class MenuItem
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty; // "phần", "ly", "tô"
        public bool IsAvailable { get; set; } = true;
        public ICollection<TicketLine> TicketLines { get; set; } = new List<TicketLine>();
    }
}
