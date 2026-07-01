using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPO_Workflow.DAL;
using NPO_Workflow.DAL.Models;
using NPO_Workflow.Extensions;
using NPO_Workflow.ViewModels;
using NPO_Workflow.ViewModels.Orders;

namespace NPO_Workflow.Controllers
{
    // [Authorize(Roles = @"DESKTOP-RDH7AFR\docker-users")]
    public class OrdersController : Controller
    {
        private readonly NPOContext _context;
        public async Task<IActionResult> Index(string search, int page = 1, string sortBy = "Id", bool? sortOrder = null)
        {
            int pageSize = 10;
            if (page < 1) page = 1;
            var query = _context.Orders.Where(item => item.isDeleted == false);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(or => or.Name.ToLower().Contains(searchLower)
                                       || or.InternationalName.ToLower().Contains(searchLower)
                                       || or.Comment.ToLower().Contains(searchLower));
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var items = await query
                .OrderByDynamic(sortBy, sortOrder ?? false)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(vm => new OrderViewModel
                {
                    Id = vm.Id,
                    Name = vm.Name,
                    InternationalName = vm.InternationalName,
                    Comment = vm.Comment
                })
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
                new() { Key = "InternationalName", Label = "Наименование (иностр.)" },
                new() { Key = "Comment", Label = "Комментарий" },
            };
            return View(items);
        }
        public OrdersController(NPOContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("Views/Orders/_CreatePartial.cshtml", new OrderViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Views/Orders/_CreatePartial.cshtml", model);
            }
            var newItem = new Order
            {
                Name = model.Name,
                InternationalName = model.InternationalName,
                Comment = model.Comment,
                isDeleted = false
            };
            _context.Orders.Add(newItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Modify(int id)
        {
            var targetItem = await _context.Orders.FirstOrDefaultAsync(item => item.Id == id);
            if (targetItem == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найдена или удалена." });
            }
            var model = new OrderViewModel()
            {
                Id = targetItem.Id,
                Name = targetItem.Name,
                InternationalName = targetItem.InternationalName,
                Comment = targetItem.Comment
            };
            return PartialView("Views/Orders/_ModifyPartial.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> Modify(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Views/Orders/_ModifyPartial.cshtml", model);
            }
            var dbOperation = await _context.Orders.FindAsync(model.Id);
            if (dbOperation == null)
            {
                return NotFound();
            }
            dbOperation.Name = model.Name;
            dbOperation.InternationalName = model.InternationalName;
            dbOperation.Comment = model.Comment;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var targetOrder = await _context.Orders.FirstOrDefaultAsync(order => order.Id == id);
            if (targetOrder == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найдена или уже удалена." });
            }
            targetOrder.isDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
