using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FCanteen.Web.Attributes;

namespace FCanteen.Web.ViewModels
{
    public class MenuItemViewModel
    {
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "MÃ£ mÃ³n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        [RegularExpression(@"^MON-\d{4}$", ErrorMessage = "MÃ£ mÃ³n pháº£i Ä‘Ãºng Ä‘á»‹nh dáº¡ng MON-xxxx (VÃ­ dá»¥: MON-0001)")]
        [Display(Name = "MÃ£ mÃ³n")]
        public string ItemCode { get; set; } = "MON-0001";

        [Required(ErrorMessage = "TÃªn mÃ³n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "TÃªn mÃ³n pháº£i tá»« 3 Ä‘áº¿n 100 kÃ½ tá»±")]
        [Display(Name = "TÃªn mÃ³n Äƒn")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "GiÃ¡ bÃ¡n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        [Range(1000, 10000000, ErrorMessage = "GiÃ¡ bÃ¡n pháº£i tá»« 1.000Ä‘ Ä‘áº¿n 10.000.000Ä‘")]
        [MinMargin(0.20)]
        [Display(Name = "GiÃ¡ bÃ¡n (VND)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "ÄÆ¡n vá»‹ tÃ­nh khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng")]
        [Display(Name = "ÄÆ¡n vá»‹ tÃ­nh")]
        public string Unit { get; set; } = "pháº§n";

        [Display(Name = "CÃ²n bÃ¡n")]
        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Vui lÃ²ng chá»n nhÃ³m mÃ³n")]
        [Display(Name = "NhÃ³m mÃ³n")]
        public int? CategoryId { get; set; }

        [Display(Name = "TÃªn nhÃ³m")]
        public string? CategoryName { get; set; }

        [Display(Name = "GiÃ¡ vá»‘n Æ°á»›c tÃ­nh (VND)")]
        public decimal CalculatedCostPrice { get; set; }
    }

    public class MenuItemIndexViewModel
    {
        public List<MenuItemViewModel> Items { get; set; } = new();
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsAvailable { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int TotalItems => Items.Count;
    }

    public class CartItemViewModel
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal Total => Quantity * UnitPrice;
    }

    public class IngredientSelectionItem
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public bool IsSelected { get; set; }
        public decimal Quantity { get; set; }
    }

    public class AssignIngredientsViewModel
    {
        public int MenuItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string MenuItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal CurrentCostPrice { get; set; }
        public List<IngredientSelectionItem> Ingredients { get; set; } = new();
    }
}