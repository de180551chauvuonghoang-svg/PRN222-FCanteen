using System.Threading.Tasks;

namespace FCanteen.Services.Interfaces
{
    public interface IReportService
    {
        Task<string> GetSummaryReportAsync();
    }
}