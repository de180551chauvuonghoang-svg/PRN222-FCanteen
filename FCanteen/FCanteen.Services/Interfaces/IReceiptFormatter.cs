using FCanteen.Data.Entities;

namespace FCanteen.Services.Interfaces
{
    public interface IReceiptFormatter
    {
        string Format(OrderTicket ticket);
    }
}