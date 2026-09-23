using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data.Entities;
using FCanteen.Repositories.Interfaces;
using FCanteen.Services.Context;
using FCanteen.Services.Interfaces;

namespace FCanteen.Services.Implementations
{
    public class OrderService : IOrderService
    {
        // 1. Constructor Injection (Bat buoc)
        private readonly IOrderRepository _orderRepo;
        private readonly IMenuItemRepository _menuRepo;
        private readonly IEnumerable<IDiscountPolicy> _discountPolicies;
        private readonly INotificationService _notificationService;

        // 2. Property Injection (Khong bat buoc)
        public IAuditLogger? AuditLogger { get; set; }

        public OrderService(
            IOrderRepository orderRepo,
            IMenuItemRepository menuRepo,
            IEnumerable<IDiscountPolicy> discountPolicies,
            INotificationService notificationService)
        {
            _orderRepo = orderRepo;
            _menuRepo = menuRepo;
            _discountPolicies = discountPolicies.OrderBy(p => p.Priority).ToList();
            _notificationService = notificationService;
        }

        public async Task<OrderTicket> CheckoutAsync(string stationName, List<(int MenuItemId, int Quantity)> items)
        {
            // 4. Ambient Context: Lay thong tin nhan vien dang truc ma khong can truyen tham so
            var currentStaff = StaffContext.Current;
            string staffInfo = currentStaff != null ? $"{currentStaff.FullName} ({currentStaff.StaffCode})" : "Unknown";

            AuditLogger?.LogAction("CHECKOUT_START", $"Tram: {stationName}, Nhan vien: {staffInfo}, Mon: {items.Count}");

            decimal rawTotal = 0;
            var ticketLines = new List<TicketLine>();

            foreach (var item in items)
            {
                var menuItem = await _menuRepo.GetByIdAsync(item.MenuItemId);
                if (menuItem == null) continue;

                decimal unitPrice = menuItem.Price;
                rawTotal += unitPrice * item.Quantity;

                ticketLines.Add(new TicketLine
                {
                    MenuItemId = menuItem.MenuItemId,
                    MenuItem = menuItem,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });
            }

            var ticket = new OrderTicket
            {
                StationName = stationName,
                BranchCode = currentStaff?.BranchCode ?? "CS1",
                TotalAmount = rawTotal,
                CreatedAt = DateTime.Now,
                Status = "Completed",
                TicketLines = ticketLines
            };

            // YC2: Ap dung danh sach cac chinh sach giam gia theo Open/Closed (OCP)
            decimal totalDiscount = 0;
            foreach (var policy in _discountPolicies)
            {
                decimal discount = policy.CalculateDiscount(ticket, currentStaff, out string reason);
                if (discount > 0)
                {
                    totalDiscount += discount;
                    AuditLogger?.LogAction("DISCOUNT_APPLIED", $"Chinh sach: {policy.PolicyName} - Giam: {discount:N0} VND ({reason})");

                    // Ghi log giam gia xuong Database theo de bai
                    await _orderRepo.AddDiscountLogAsync(new DiscountPolicyLog
                    {
                        OrderTicketId = ticket.OrderTicketId,
                        PolicyName = policy.PolicyName,
                        DiscountAmount = discount,
                        AppliedAt = DateTime.Now,
                        Reason = reason
                    });
                }
            }

            // Cap nhat tong tien sau giam gia
            ticket.TotalAmount = Math.Max(0, rawTotal - totalDiscount);

            // Luu order vao CSDL
            await _orderRepo.CreateOrderAsync(ticket);

            // YC3: Gui thong bao qua kenh INotificationService duoc inject
            await _notificationService.SendNotificationAsync(
                "Don hang moi thanh cong",
                $"Phieu #{ticket.OrderTicketId} | Tram: {stationName} | Tong: {ticket.TotalAmount:N0} VND (Giam: {totalDiscount:N0} VND)");

            AuditLogger?.LogAction("CHECKOUT_SUCCESS", $"Phieu #{ticket.OrderTicketId} da luu vao CSDL. Tong: {ticket.TotalAmount:N0} VND");

            return ticket;
        }

        // 3. Method Injection: Chi can formatter khi thuc su xuat hoa don
        public void ExportReceipt(OrderTicket ticket, IReceiptFormatter formatter)
        {
            string output = formatter.Format(ticket);
            Console.WriteLine(output);
            AuditLogger?.LogAction("EXPORT_RECEIPT", $"Xuat hoa don cho phieu #{ticket.OrderTicketId} bang {formatter.GetType().Name}");
        }
    }
}