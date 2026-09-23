using System.Threading.Tasks;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IOrderRepository _orderRepo;

        public ReportService(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        public async Task<string> GetSummaryReportAsync()
        {
            var recent = await _orderRepo.GetRecentTicketsAsync(5);
            return $"Bao cao: Co {recent.Count} phieu gan nhat duoc load qua IOrderRepository.";
        }
    }
}