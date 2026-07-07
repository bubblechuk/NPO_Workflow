using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPO_Workflow.DAL;
using NPO_Workflow.DAL.Models;
using NPO_Workflow.Extensions;
using NPO_Workflow.ViewModels;
using NPO_Workflow.ViewModels.Details;

namespace NPO_Workflow.Controllers
{
    // [Authorize(Roles = @"DESKTOP-RDH7AFR\docker-users")]
    public class DetailsController : Controller
    {
        private readonly NPOContext _context;
        public async Task<IActionResult> Index(string search, int page = 1, string sortBy = "Id", bool? sortOrder = null)
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = from de in _context.Details.Where(d => !d.IsDeleted)
                        join or in _context.Orders.Where(o => !o.IsDeleted) on de.OrderId equals or.Id into orderJoin
                        from subOrder in orderJoin.DefaultIfEmpty()
                        join pDe in _context.Details on de.ParentId equals pDe.Id into parentJoin
                        from subParent in parentJoin.DefaultIfEmpty()
                        select new DetailViewModel
                        {
                            Id = de.Id,
                            Name = de.Name,
                            OrderId = de.OrderId,
                            ParentId = de.ParentId,
                            Quantity = de.Quantity,
                            OrderName = subOrder != null ? subOrder.Name : "-",
                            ParentDetailName = subParent != null ? subParent.Name : "-"
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();

                query = query.Where(vm => 
                    (vm.Name != null && vm.Name.ToLower().Contains(searchLower))
                    || (vm.OrderName != null && vm.OrderName.ToLower().Contains(searchLower))
                    || (vm.ParentDetailName != null && vm.ParentDetailName.ToLower().Contains(searchLower))
                );
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var items = await query
                .OrderByDynamic(sortBy, sortOrder ?? false)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Columns = new List<SortColumn>
    {
        new() { Key = "Id", Label = "ID" },
        new() { Key = "Name", Label = "Наименование" },
        new() { Key = "OrderId", Label = "Заказ" },
        new() { Key = "ParentId", Label = "Род. деталь" },
        new() { Key = "Quantity", Label = "Количество"}
    };

            return View(items);
        }
        public DetailsController(NPOContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var orders = await _context.Orders
                .Where(o => !o.IsDeleted)
                .ToListAsync();

            var parentDetails = await _context.Details
                .Where(d => !d.IsDeleted)
                .ToListAsync();
            var viewModel = new DetailViewModel()
            {
                OrdersList = new SelectList(orders, "Id", "Name"),
                ParentDetailsList = new SelectList(parentDetails, "Id", "Name")
            };
            return PartialView("Views/Details/_CreatePartial.cshtml", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/Details/_CreatePartial.cshtml", model);
            }
            if (model.ParentId.HasValue)
            {
                var parentDetail = await _context.Details.FirstOrDefaultAsync(d => d.Id == model.ParentId && !d.IsDeleted);
                if (parentDetail == null)
                {
                    ModelState.AddModelError("ParentId", "Указанная родительская деталь не найдена.");
                }
                else if (parentDetail.OrderId != model.OrderId)
                {
                    ModelState.AddModelError("ParentId", "Родительская деталь должна принадлежать тому же заказу, что и текущая.");
                }
            }

            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/Details/_CreatePartial.cshtml", model);
            }

            var newItem = new Detail
            {
                Name = model.Name,
                OrderId = model.OrderId ?? 0,
                ParentId = model.ParentId,
                Quantity = model.Quantity,
                IsDeleted = false
            };

            _context.Details.Add(newItem);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task RebuildSelectListsAsync(DetailViewModel model)
        {
            var orders = await _context.Orders.Where(o => !o.IsDeleted).ToListAsync();
            var parentDetails = await _context.Details.Where(d => !d.IsDeleted && d.Id != model.Id).ToListAsync();

            model.OrdersList = new SelectList(orders, "Id", "Name");
            model.ParentDetailsList = new SelectList(parentDetails, "Id", "Name");
        }
        [HttpGet]
        public async Task<IActionResult> Modify(int id)
        {
            var targetItem = await _context.Details.FirstOrDefaultAsync(item => item.Id == id);
            if (targetItem == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найдена или удалена." });
            }
            var orders = await _context.Orders
                .Where(o => !o.IsDeleted)
                .ToListAsync();

            var parentDetails = await _context.Details
                .Where(d => !d.IsDeleted)
                .ToListAsync();
            var model = new DetailViewModel()
            {
                Id = targetItem.Id,
                Name = targetItem.Name,
                OrderId = targetItem.OrderId,
                ParentId = targetItem.ParentId,
                Quantity = targetItem.Quantity,
                OrdersList = new SelectList(orders, "Id", "Name"),
                ParentDetailsList = new SelectList(parentDetails, "Id", "Name")
            };
            return PartialView("Views/Details/_ModifyPartial.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modify(DetailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/Details/_ModifyPartial.cshtml", model);
            }

            var dbOperation = await _context.Details.FindAsync(model.Id);
            if (dbOperation == null || dbOperation.IsDeleted)
            {
                return NotFound(new { message = "Редактируемая деталь не найдена." });
            }
            if (model.ParentId == model.Id)
            {
                ModelState.AddModelError("ParentId", "Деталь не может быть родительской для самой себя.");
            }
            if (model.ParentId.HasValue && ModelState.IsValid)
            {
                var parentDetail = await _context.Details.FirstOrDefaultAsync(d => d.Id == model.ParentId && !d.IsDeleted);
                if (parentDetail == null)
                {
                    ModelState.AddModelError("ParentId", "Указанная родительская деталь не найдена.");
                }
                else
                {
                    if (parentDetail.OrderId != model.OrderId)
                    {
                        ModelState.AddModelError("ParentId", "Родительская деталь должна принадлежать тому же заказу.");
                    }
                    var currentCheck = parentDetail;
                    while (currentCheck != null)
                    {
                        if (currentCheck.Id == model.Id)
                        {
                            ModelState.AddModelError("ParentId", $"Невозможно назначить деталь родителем, так как она является дочерней для текущей (возникнет циклическая ссылка).");
                            break;
                        }
                        if (currentCheck.ParentId.HasValue)
                        {
                            currentCheck = await _context.Details.AsNoTracking().FirstOrDefaultAsync(d => d.Id == currentCheck.ParentId);
                        }
                        else
                        {
                            currentCheck = null;
                        }
                    }
                }
            }
            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/Details/_ModifyPartial.cshtml", model);
            }
            if (dbOperation.OrderId != model.OrderId)
            {
                await UpdateChildrenOrderIdAsync(dbOperation.Id, model.OrderId ?? 0);
            }
            dbOperation.Name = model.Name;
            dbOperation.OrderId = model.OrderId ?? 0;
            dbOperation.ParentId = model.ParentId;
            dbOperation.Quantity = model.Quantity;
            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Ошибка параллельного доступа. Попробуйте еще раз.");
                await RebuildSelectListsAsync(model);
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var target = await _context.Details.FirstOrDefaultAsync(item => item.Id == id);
            if (target == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найдена или уже удалена." });
            }
            target.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
        private async Task UpdateChildrenOrderIdAsync(int parentId, int newOrderId)
        {
            var childDetails = await _context.Details.Where(d => d.ParentId == parentId).ToListAsync();
            foreach (var detail in childDetails)
            {
                detail.OrderId = newOrderId;
                await UpdateChildrenOrderIdAsync(detail.Id, newOrderId);
            }
        }
    }
}
