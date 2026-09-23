using System;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Audit
{
    public class ConsoleAuditLogger : IAuditLogger
    {
        public void LogAction(string action, string detail)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[AUDIT LOG] {DateTime.Now:HH:mm:ss} | {action.ToUpper()} | {detail}");
            Console.ResetColor();
        }
    }
}