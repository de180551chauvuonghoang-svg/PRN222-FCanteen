using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FCanteen.Data;
using FCanteen.Data.Entities;
using FCanteen.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FCanteen.Web.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly FCanteenContext _context;

        public MenuItemsController(FCanteenContext context)
        {
            _context = context;
        }

        // =====================================================================
        // YC3: TIM KIEM, LOC, SAP XEP VA 4 CACH TRUYEN DU LIEU
        // =====================================================================
        public async Task<IActionResult> Index(
            string? searchString,
            int? categoryId,
            bool? isAvailable,
            string sortBy = "Name",
            string sortOrder = "asc")
        {
            // 1. Khoi tao query
            var query = _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.MenuItemIngredients)
                .ThenInclude(mi => mi.Ingredient)
                .AsNoTracking()
                .AsQueryable();

            // 2. Tim kiem theo Ten hoac Ma mon
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string term = searchString.Trim().ToLower();
                query = query.Where(m => m.Name.ToLower().Contains(term) || m.ItemCode.ToLower().Contains(term));
            }

            // 3. Loc theo Nhom mon (Category)
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(m => m.CategoryId == categoryId.Value);
            }

            // 4. Loc theo Trang thai con ban (isAvailable)
            if (isAvailable.HasValue)
            {
                query = query.Where(m => m.IsAvailable == isAvailable.Value);
            }

            // 5. Sap xep hai chieu (Sort By Name / Price)
            bool isDesc = sortOrder?.ToLower() == "desc";
            query = sortBy switch
            {
                "Price" => isDesc ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price),
                "ItemCode" => isDesc ? query.OrderByDescending(m => m.ItemCode) : query.OrderBy(m => m.ItemCode),
                _ => isDesc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name)
            };

            var items = await query.Select(m => new MenuItemViewModel
            {
                MenuItemId = m.MenuItemId,
                ItemCode = m.ItemCode,
                Name = m.Name,
                Price = m.Price,
                Unit = m.Unit,
                IsAvailable = m.IsAvailable,
                CategoryId = m.CategoryId,
                CategoryName = m.Category != null ? m.Category.Name : "ChÆ°a phÃ¢n loáº¡i",
                CalculatedCostPrice = m.MenuItemIngredients.Sum(mi => mi.Quantity * mi.Ingredient.CostPrice)
            }).ToListAsync();

            // -----------------------------------------------------------------
            // CHUNG MINH 4 CACH TRUYEN DU LIEU TRONG CUNG LAB:
            // -----------------------------------------------------------------
            // Cach 1: ViewData truyen danh sach Category cho Dropdown
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "Name", categoryId);

            // Cach 2: ViewBag truyen trang thai dong de dao chieu sort va thong so thong ke
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.NextNameSortOrder = (sortBy == "Name" && sortOrder == "asc") ? "desc" : "asc";
            ViewBag.NextPriceSortOrder = (sortBy == "Price" && sortOrder == "asc") ? "desc" : "asc";
            ViewBag.TotalItemsCount = items.Count;

            // Cach 3: Model Strongly-typed truyen toan bo du lieu va filter params
            var viewModel = new MenuItemIndexViewModel
            {
                Items = items,
                SearchString = searchString,
                CategoryId = categoryId,
                IsAvailable = isAvailable,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // (Cach 4: TempData duoc doc tai View thong qua _Layout de hien thi thong bao)
            return View(viewModel);
        }

        // =====================================================================
        // YC2: CRUD THUC DON CHI TIET
        // =====================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.MenuItemIngredients)
                .ThenInclude(mi => mi.Ingredient)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (menuItem == null) return NotFound();

            var vm = new MenuItemViewModel
            {
                MenuItemId = menuItem.MenuItemId,
                ItemCode = menuItem.ItemCode,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Unit = menuItem.Unit,
                IsAvailable = menuItem.IsAvailable,
                CategoryId = menuItem.CategoryId,
                CategoryName = menuItem.Category?.Name,
                CalculatedCostPrice = menuItem.MenuItemIngredients.Sum(mi => mi.Quantity * mi.Ingredient.CostPrice)
            };

            return View(vm);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(new MenuItemViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItemViewModel model)
        {
            // Server-side validation: Kiem tra trung Ma mon
            if (await _context.MenuItems.AnyAsync(m => m.ItemCode.ToLower() == model.ItemCode.Trim().ToLower()))
            {
                ModelState.AddModelError("ItemCode", $"MÃ£ mÃ³n '{model.ItemCode}' Ä‘Ã£ tá»“n táº¡i trong há»‡ thá»‘ng. Vui lÃ²ng chá»n mÃ£ khÃ¡c!");
            }

            if (ModelState.IsValid)
            {
                var menuItem = new MenuItem
                {
                    ItemCode = model.ItemCode.Trim().ToUpper(),
                    Name = model.Name.Trim(),
                    Price = model.Price,
                    Unit = model.Unit.Trim(),
                    IsAvailable = model.IsAvailable,
                    CategoryId = model.CategoryId
                };

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                // Cach 4: TempData truyen thong bao sau khi redirect (PRG pattern)
                TempData["SuccessMessage"] = $"ThÃªm mÃ³n '{menuItem.Name}' ({menuItem.ItemCode}) thÃ nh cÃ´ng!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.MenuItemIngredients)
                .ThenInclude(mi => mi.Ingredient)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (menuItem == null) return NotFound();

            var vm = new MenuItemViewModel
            {
                MenuItemId = menuItem.MenuItemId,
                ItemCode = menuItem.ItemCode,
                Name = menuItem.Name,
                Price = menuItem.Price,
                Unit = menuItem.Unit,
                IsAvailable = menuItem.IsAvailable,
                CategoryId = menuItem.CategoryId,
                CalculatedCostPrice = menuItem.MenuItemIngredients.Sum(mi => mi.Quantity * mi.Ingredient.CostPrice)
            };

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", menuItem.CategoryId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItemViewModel model)
        {
            if (id != model.MenuItemId) return NotFound();

            // Server-side validation: Kiem tra trung Ma mon voi mon khac
            if (await _context.MenuItems.AnyAsync(m => m.ItemCode.ToLower() == model.ItemCode.Trim().ToLower() && m.MenuItemId != id))
            {
                ModelState.AddModelError("ItemCode", $"MÃ£ mÃ³n '{model.ItemCode}' Ä‘Ã£ Ä‘Æ°á»£c sá»­ dá»¥ng bá»Ÿi mÃ³n Äƒn khÃ¡c!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var menuItem = await _context.MenuItems.FindAsync(id);
                    if (menuItem == null) return NotFound();

                    menuItem.ItemCode = model.ItemCode.Trim().ToUpper();
                    menuItem.Name = model.Name.Trim();
                    menuItem.Price = model.Price;
                    menuItem.Unit = model.Unit.Trim();
                    menuItem.IsAvailable = model.IsAvailable;
                    menuItem.CategoryId = model.CategoryId;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cáº­p nháº­t mÃ³n '{menuItem.Name}' thÃ nh cÃ´ng!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MenuItems.Any(e => e.MenuItemId == id))
                        return NotFound();
                    throw;
                }
            }

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"ÄÃ£ xÃ³a mÃ³n '{menuItem.Name}' khá»i thá»±c Ä‘Æ¡n!";
            }
            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        // YC5: GAN NGUYEN LIEU KEM DINH LUONG VA TU TINH GIA VON
        // =====================================================================
        public async Task<IActionResult> AssignIngredients(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.MenuItemIngredients)
                .FirstOrDefaultAsync(m => m.MenuItemId == id);

            if (menuItem == null) return NotFound();

            var allIngredients = await _context.Ingredients.AsNoTracking().ToListAsync();

            var assignedDict = menuItem.MenuItemIngredients
                .ToDictionary(mi => mi.IngredientId, mi => mi.Quantity);

            var vm = new AssignIngredientsViewModel
            {
                MenuItemId = menuItem.MenuItemId,
                ItemCode = menuItem.ItemCode,
                MenuItemName = menuItem.Name,
                Price = menuItem.Price,
                Ingredients = allIngredients.Select(i => new IngredientSelectionItem
                {
                    IngredientId = i.IngredientId,
                    Name = i.Name,
                    Unit = i.Unit,
                    CostPrice = i.CostPrice,
                    IsSelected = assignedDict.ContainsKey(i.IngredientId),
                    Quantity = assignedDict.GetValueOrDefault(i.IngredientId, 0m)
                }).ToList()
            };

            vm.CurrentCostPrice = vm.Ingredients.Where(i => i.IsSelected).Sum(i => i.Quantity * i.CostPrice);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignIngredients(AssignIngredientsViewModel model)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.MenuItemIngredients)
                .FirstOrDefaultAsync(m => m.MenuItemId == model.MenuItemId);

            if (menuItem == null) return NotFound();

            // Xoa cac dinh luong cu
            _context.MenuItemIngredients.RemoveRange(menuItem.MenuItemIngredients);

            // Them cac dinh luong moi duoc chon va co so luong > 0
            foreach (var item in model.Ingredients.Where(i => i.IsSelected && i.Quantity > 0))
            {
                menuItem.MenuItemIngredients.Add(new MenuItemIngredient
                {
                    MenuItemId = menuItem.MenuItemId,
                    IngredientId = item.IngredientId,
                    Quantity = item.Quantity
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"ÄÃ£ cáº­p nháº­t Ä‘á»‹nh lÆ°á»£ng nguyÃªn liá»‡u cho mÃ³n '{menuItem.Name}' thÃ nh cÃ´ng!";
            return RedirectToAction(nameof(Details), new { id = menuItem.MenuItemId });
        }

        // =====================================================================
        // YC5: ENDPOINT JSON TRA VE MON CON BAN (PHUC VU POSCLIENT QUA HTTPCLIENT)
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> GetAvailableMenuItems()
        {
            var items = await _context.MenuItems
                .Where(m => m.IsAvailable)
                .Include(m => m.Category)
                .AsNoTracking()
                .Select(m => new
                {
                    m.MenuItemId,
                    m.ItemCode,
                    m.Name,
                    m.Price,
                    m.Unit,
                    Category = m.Category != null ? m.Category.Name : "KhÃ¡c"
                })
                .ToListAsync();

            return Json(new
            {
                Total = items.Count,
                ServerTime = DateTime.Now,
                Items = items
            });
        }
    }
}