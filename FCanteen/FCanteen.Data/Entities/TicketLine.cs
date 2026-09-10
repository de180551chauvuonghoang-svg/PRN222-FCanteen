using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.Data.Entities
{
    public class TicketLine
    {
        public int TicketLineId { get; set; }
        public int OrderTicketId { get; set; }
        public OrderTicket OrderTicket { get; set; } = null!;
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }// Giá tại thời điểm bán, KHÔNG đọc từ MenuItem
        public string Note { get; set; } = string.Empty;
    }

}

