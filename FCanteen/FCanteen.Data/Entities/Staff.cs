using System;

namespace FCanteen.Data.Entities
{
    public class Staff
    {
        public int StaffId { get; set; }
        public string StaffCode { get; set; } = string.Empty; // NV01, NV02...
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Cashier";        // Cashier, Manager, Chef
        public string BranchCode { get; set; } = "CS1";       // CS1, CS2, CS3
    }
}