using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Web.Helpers;
using FCanteen.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Web.Controllers
{
    public class IngredientCartController : Controller
    {
        private const string CART_SESSION_KEY = "IngredientCart";
        private readonly FCanteenContext _context;

        public IngredientCartController(FCanteenContext context)
        {
            _context = context;
        }

        // Xem danh sach nguyen lieu de them vao gio hang
        public async Task<IActionResult> IngredientsList()
        {
            var ingredients = await _context.Ingredients
                .Include(i => i.Supplier)
                .AsNoTracking()
                .ToListAsync();

            return View(ingredients);
        }

        // Xem gio dat hang hien tai trong Session
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetJson<List<CartItemViewModel>>(CART_SESSION_KEY) ?? new List<CartItemViewModel>();
            return View(cart);
        }

        // Them nguyen lieu vao gio
        [HttpPost]
        public async Task<IActionResult> AddToCart(int ingredientId, decimal quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var ingredient = await _context.Ingredients
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.IngredientId == ingredientId);

            if (ingredient == null) return NotFound();

            var cart = HttpContext.Session.GetJson<List<CartItemViewModel>>(CART_SESSION_KEY) ?? new List<CartItemViewModel>();
            var item = cart.FirstOrDefault(c => c.IngredientId == ingredientId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    IngredientId = ingredient.IngredientId,
                    IngredientName = ingredient.Name,
                    Unit = ingredient.Unit,
                    UnitPrice = ingredient.CostPrice,
                    Quantity = quantity,
                    SupplierId = ingredient.SupplierId ?? 1,
                    SupplierName = ingredient.Supplier?.Name ?? "NhÃ  cung cáº¥p chÃ­nh"
                });
            }

            HttpContext.Session.SetJson(CART_SESSION_KEY, cart);
            TempData["SuccessMessage"] = $"ÄÃ£ thÃªm {quantity} {ingredient.Unit} '{ingredient.Name}' vÃ o giá» Ä‘áº·t hÃ ng!";

            return RedirectToAction(nameof(IngredientsList));
        }

        // Cap nhat so luong
        [HttpPost]
        public IActionResult UpdateQuantity(int ingredientId, decimal quantity)
        {
            var cart = HttpContext.Session.GetJson<List<CartItemViewModel>>(CART_SESSION_KEY) ?? new List<CartItemViewModel>();
            var item = cart.FirstOrDefault(c => c.IngredientId == ingredientId);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Remove(item);
                }
                HttpContext.Session.SetJson(CART_SESSION_KEY, cart);
                TempData["SuccessMessage"] = "ÄÃ£ cáº­p nháº­t sá»‘ lÆ°á»£ng thÃ nh cÃ´ng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Xoa dong khoi gio
        [HttpPost]
        public IActionResult RemoveFromCart(int ingredientId)
        {
            var cart = HttpContext.Session.GetJson<List<CartItemViewModel>>(CART_SESSION_KEY) ?? new List<CartItemViewModel>();
            var item = cart.FirstOrDefault(c => c.IngredientId == ingredientId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetJson(CART_SESSION_KEY, cart);
                TempData["SuccessMessage"] = $"ÄÃ£ xÃ³a '{item.IngredientName}' khá»i giá» Ä‘áº·t hÃ ng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Xac nhan dat hang: Tao PurchaseOrder, luu vao DB va xoa Session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetJson<List<CartItemViewModel>>(CART_SESSION_KEY);
            if (cart == null || cart.Count == 0)
            {
                TempData["ErrorMessage"] = "Giá» hÃ ng rá»—ng, khÃ´ng thá»ƒ táº¡o Ä‘Æ¡n Ä‘áº·t hÃ ng!";
                return RedirectToAction(nameof(Index));
            }

            // Nhom theo nha cung cap de tao phieu dat hang
            var order = new PurchaseOrder
            {
                OrderDate = DateTime.Now,
                SupplierId = cart.First().SupplierId,
                Status = "Completed",
                TotalAmount = cart.Sum(c => c.Total),
                Lines = cart.Select(c => new PurchaseOrderLine
                {
                    IngredientId = c.IngredientId,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice
                }).ToList()
            };

            // Cong them ton kho nguyen lieu tuong ung
            foreach (var line in cart)
            {
                var ing = await _context.Ingredients.FindAsync(line.IngredientId);
                if (ing != null)
                {
                    ing.StockQuantity += (int)line.Quantity;
                }
            }

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();

            // Xoa sach Session sau khi dat hang thanh cong
            HttpContext.Session.Remove(CART_SESSION_KEY);

            TempData["SuccessMessage"] = $"Táº¡o phiáº¿u Ä‘áº·t hÃ ng #{order.PurchaseOrderId} thÃ nh cÃ´ng! Tá»•ng tiá»n: {order.TotalAmount:N0}Ä‘ (ÄÃ£ cáº­p nháº­t tá»“n kho).";
            return RedirectToAction(nameof(Index));
        }
    }
}