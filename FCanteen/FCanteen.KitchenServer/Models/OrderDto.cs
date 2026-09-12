using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCanteen.KitchenServer.Models
{
    // DTO nhan tu PosClient gui len
    public class OrderDto
    {
        public string StationName { get; set; } = "";
        public List<OrderLineDto> Lines { get; set; } = new();
    }
    public class OrderLineDto
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; } = "";
    }
    // DTO gui xac nhan ve PosClient
    public class OrderConfirmDto
    {
        public int OrderTicketId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = "";
    }
}
