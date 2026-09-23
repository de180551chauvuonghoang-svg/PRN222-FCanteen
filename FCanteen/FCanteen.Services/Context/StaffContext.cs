using System.Threading;
using FCanteen.Data.Entities;

namespace FCanteen.Services.Context
{
    public static class StaffContext
    {
        private static readonly AsyncLocal<Staff?> _currentStaff = new();

        public static Staff? Current
        {
            get => _currentStaff.Value;
            set => _currentStaff.Value = value;
        }
    }
}