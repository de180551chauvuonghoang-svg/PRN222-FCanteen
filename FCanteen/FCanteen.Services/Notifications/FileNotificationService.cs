using System;
using System.IO;
using System.Threading.Tasks;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Notifications
{
    public class FileNotificationService : INotificationService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notifications.log");

        public async Task SendNotificationAsync(string title, string message)
        {
            string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{title}] {message}{Environment.NewLine}";
            await File.AppendAllTextAsync(_filePath, logLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[FILE THONG BAO] Da ghi vao log: {_filePath}");
            Console.ResetColor();
        }
    }
}