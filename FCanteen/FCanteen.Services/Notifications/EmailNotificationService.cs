using System;
using System.Threading.Tasks;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Notifications
{
    public class EmailNotificationService : INotificationService
    {
        public Task SendNotificationAsync(string title, string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[EMAIL GIA LAP] Dang gui email den: canteenfpt@fpt.edu.vn");
            Console.WriteLine($"  Tieu de: [FCanteen] {title}");
            Console.WriteLine($"  Body: {message}");
            Console.WriteLine($"  >> Email da duoc gui thanh cong qua SMTP 587!");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}