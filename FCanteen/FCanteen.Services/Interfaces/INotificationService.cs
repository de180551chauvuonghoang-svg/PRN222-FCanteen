using System.Threading.Tasks;

namespace FCanteen.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string title, string message);
    }
}