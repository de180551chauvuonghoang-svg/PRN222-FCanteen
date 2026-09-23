using System.Text;
using System.Text.Json;
using FCanteen.Data.Entities;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Formatters
{
    public class PlainTextReceiptFormatter : IReceiptFormatter
    {
        public string Format(OrderTicket ticket)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=====================================");
            sb.AppendLine($"    HOA DON PHIEU #{ticket.OrderTicketId}");
            sb.AppendLine($"    Tram: {ticket.StationName} | Co so: {ticket.BranchCode}");
            sb.AppendLine($"    Ngay: {ticket.CreatedAt:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine("-------------------------------------");
            foreach (var l in ticket.TicketLines)
            {
                string itemName = l.MenuItem != null ? l.MenuItem.Name : $"Mon #{l.MenuItemId}";
                sb.AppendLine($" - {itemName} x{l.Quantity}: {l.Quantity * l.UnitPrice:N0} VND");
            }
            sb.AppendLine("-------------------------------------");
            sb.AppendLine($" TONG CONG: {ticket.TotalAmount:N0} VND");
            sb.AppendLine("=====================================");
            return sb.ToString();
        }
    }

    public class JsonReceiptFormatter : IReceiptFormatter
    {
        public string Format(OrderTicket ticket)
        {
            return JsonSerializer.Serialize(new
            {
                ticket.OrderTicketId,
                ticket.BranchCode,
                ticket.StationName,
                ticket.TotalAmount,
                ticket.CreatedAt,
                Lines = ticket.TicketLines
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}