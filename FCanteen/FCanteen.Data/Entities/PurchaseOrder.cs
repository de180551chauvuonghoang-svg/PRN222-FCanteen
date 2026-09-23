using System;
using System.Collections.Generic;

namespace FCanteen.Data.Entities
{
    public class PurchaseOrder
    {
        public int PurchaseOrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public string Status { get; set; } = "Completed";
        public decimal TotalAmount { get; set; }
        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    }

    public class PurchaseOrderLine
    {
        public int PurchaseOrderLineId { get; set; }
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }
        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}