using System.Collections.Generic;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Interfaces
{
    public interface IOrderService
    {
        IAuditLogger? AuditLogger { get; set; } // Property Injection
        Task<OrderTicket> CheckoutAsync(string stationName, List<(int MenuItemId, int Quantity)> items);
        void ExportReceipt(OrderTicket ticket, IReceiptFormatter formatter); // Method Injection
    }
}