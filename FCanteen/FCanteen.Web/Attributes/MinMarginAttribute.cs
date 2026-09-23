using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FCanteen.Data;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Web.Attributes
{
    public class MinMarginAttribute : ValidationAttribute
    {
        private readonly double _minMarginRate;

        public MinMarginAttribute(double minMarginRate = 0.20)
        {
            _minMarginRate = minMarginRate;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not decimal price)
                return ValidationResult.Success;

            // Lay instance FCanteenContext qua DI ServiceProvider cua ValidationContext
            var context = (FCanteenContext?)validationContext.GetService(typeof(FCanteenContext));
            if (context == null) return ValidationResult.Success;

            // Lay MenuItemId tu Model dang xet
            var property = validationContext.ObjectType.GetProperty("MenuItemId");
            int menuItemId = 0;
            if (property != null)
            {
                var val = property.GetValue(validationContext.ObjectInstance);
                if (val is int id) menuItemId = id;
            }

            // Tinh gia von tu dinh luong nguyen lieu cua mon (neu da co)
            if (menuItemId > 0)
            {
                var costPrice = context.MenuItemIngredients
                    .Where(mi => mi.MenuItemId == menuItemId)
                    .Include(mi => mi.Ingredient)
                    .AsNoTracking()
                    .Sum(mi => mi.Quantity * mi.Ingredient.CostPrice);

                if (costPrice > 0)
                {
                    decimal minRequiredPrice = costPrice * (decimal)(1.0 + _minMarginRate);
                    if (price < minRequiredPrice)
                    {
                        return new ValidationResult(
                            $"GiÃ¡ bÃ¡n ({price:N0}Ä‘) pháº£i lá»›n hÆ¡n giÃ¡ vá»‘n ({costPrice:N0}Ä‘) Ã­t nháº¥t {(_minMarginRate * 100):0}% (Tá»‘i thiá»ƒu pháº£i tá»« {minRequiredPrice:N0}Ä‘).");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}