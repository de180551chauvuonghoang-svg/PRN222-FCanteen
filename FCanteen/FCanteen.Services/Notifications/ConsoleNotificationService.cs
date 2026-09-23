using System;
using System.Threading.Tasks;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Notifications
{
    public class ConsoleNotificationService : INotificationService
    {
        public Task SendNotificationAsync(string title, string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[CONSOLE THONG BAO] === {title} ===");
            Console.WriteLine($"  Noi dung: {message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}