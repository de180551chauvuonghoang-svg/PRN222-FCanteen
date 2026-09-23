namespace FCanteen.Services.Interfaces
{
    public interface IAuditLogger
    {
        void LogAction(string action, string detail);
    }
}