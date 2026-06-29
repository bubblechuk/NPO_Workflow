using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPO_Workflow.DAL;
using NPO_Workflow.DAL.Models;
using NPO_Workflow.Extensions;
using NPO_Workflow.ViewModels;
using NPO_Workflow.ViewModels.Technologies;

namespace NPO_Workflow.Controllers
{
    public class TechnologiesController : Controller
    {
        private readonly NPOContext _context;

        public TechnologiesController(NPOContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string search, int page = 1, string sortBy = "Id", bool? sortOrder = null)
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = from te in _context.Technologies.Where(t => !t.isDeleted)
                        join de in _context.Details.Where(d => !d.isDeleted) on te.DetailId equals de.Id
                        select new TechnologyViewModel
                        {
                            Id = te.Id,
                            DetailId = te.DetailId,
                            DetailName = de.Name
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(vm => vm.DetailName.ToLower().Contains(searchLower));
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
                new() { Key = "DetailId", Label = "Наименование детали" },
            };

            return View("Technologies", items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new TechnologyViewModel();
            await RebuildSelectListsAsync(viewModel);

            return PartialView("Views/Technologies/_CreatePartial.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechnologyViewModel model)
        {
            var isDuplicate = await _context.Technologies
                .AnyAsync(t => !t.isDeleted && t.DetailId == model.DetailId);
            if (isDuplicate)
            {
                ModelState.AddModelError("DetailId", "Для данной детали уже создана технологическая карта.");
            }

            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/Technologies/_CreatePartial.cshtml", model);
            }
            var newItem = new Technology
            {
                DetailId = model.DetailId,
                isDeleted = false
            };
            _context.Technologies.Add(newItem);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Modify(int id)
        {
            var targetItem = await _context.Technologies.FirstOrDefaultAsync(item => item.Id == id);
            if (targetItem == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найден или удален." });
            }

            var model = new TechnologyViewModel()
            {
                Id = targetItem.Id,
                DetailId = targetItem.DetailId
            };

            await RebuildSelectListsAsync(model);

            return PartialView("Views/Technologies/_ModifyPartial.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Modify(TechnologyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/Technologies/_ModifyPartial.cshtml", model);
            }

            var dbOperation = await _context.Technologies.FindAsync(model.Id);
            if (dbOperation == null)
            {
                return NotFound();
            }

            dbOperation.DetailId = model.DetailId;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var target = await _context.Technologies.FirstOrDefaultAsync(tech => tech.Id == id);
            if (target == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найден или уже удален." });
            }
            target.isDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task RebuildSelectListsAsync(TechnologyViewModel model)
        {
            var busyDetailIds = await _context.Technologies
                .Where(t => !t.isDeleted)
                .Select(t => t.DetailId)
                .ToListAsync();
            var details = await _context.Details
                .Where(d => !d.isDeleted && (!busyDetailIds.Contains(d.Id) || d.Id == model.DetailId))
                .ToListAsync();

            model.DetailsList = new SelectList(details, "Id", "Name", model.DetailId);
        }

        public IActionResult Operations()
        {
            return View("TechnologyOperations");
        }
    }
}