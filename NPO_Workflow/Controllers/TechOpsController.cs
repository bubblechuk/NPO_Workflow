using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using NPO_Workflow.DAL;
using NPO_Workflow.DAL.Models;
using NPO_Workflow.Extensions;
using NPO_Workflow.ViewModels;
using NPO_Workflow.ViewModels.Technologies;
using NPO_Workflow.ViewModels.TechOps;

namespace NPO_Workflow.Controllers
{

    public class TechOpsController : Controller
    {
        private readonly NPOContext _context;
        [Route("/Technologies/Operations", Name = "TechOperationsIndex")]
        public async Task<IActionResult> Index(string search, int page = 1, string sortBy = "Id", bool? sortOrder = null)
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = from teo in _context.TechnologyOperations.Where(t => !t.isDeleted)

                        join te in _context.Technologies.Where(d => !d.isDeleted)
                            on teo.TechnologyId equals te.Id into techJoin
                        from tech in techJoin.DefaultIfEmpty()
                        join op in _context.Operations.Where(d => !d.isDeleted)
                            on teo.OperationId equals op.Id into opJoin
                        from operation in opJoin.DefaultIfEmpty()
                        join de in _context.Details.Where(d => !d.isDeleted)
                            on (tech != null ? tech.DetailId : 0) equals de.Id into detailJoin
                        from detail in detailJoin.DefaultIfEmpty()
                        select new TechOpViewModel {
                            Id = teo.Id,
                            TechnologyId = teo.TechnologyId,
                            OperationId = teo.OperationId,
                            OperationName = operation != null ? operation.Name : "-",
                            DetailName = detail != null ? detail.Name : "-"
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(vm => vm.OperationName.ToLower().Contains(searchLower) || 
                                          vm.TechnologyId.ToString().Contains(searchLower) ||
                                          vm.OperationName.ToLower().Contains(searchLower) ||
                                          vm.DetailName.ToLower().Contains(searchLower));
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
                new() { Key = "Id", Label = "ID тех. операции" },
                new() { Key = "DetailName", Label = "Наименование детали"},
                new() { Key = "OperationName", Label = "Наименование операции" }
            };

            return View(items);
        }
        public TechOpsController(NPOContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new TechOpViewModel();
            await RebuildSelectListsAsync(viewModel);

            return PartialView("Views/TechOps/_CreatePartial.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TechOpViewModel model)
        {
            if (model.TechnologyId <= 0)
            {
                ModelState.AddModelError("TechnologyId", "Необходимо выбрать деталь из списка.");
            }
            if (model.OperationId <= 0)
            {
                ModelState.AddModelError("OperationId", "Необходимо выбрать операцию из списка.");
            }

            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/TechOps/_CreatePartial.cshtml", model);
            }

            var newItem = new TechnologyOperation
            {
                TechnologyId = model.TechnologyId,
                OperationId = model.OperationId,  
                isDeleted = false
            };
            _context.TechnologyOperations.Add(newItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        private async Task RebuildSelectListsAsync(TechOpViewModel model)
        {
            var operations = await _context.Operations
                .Where(op => !op.isDeleted)
                .OrderBy(op => op.Name)
                .ToListAsync();
            var detailsWithTech = await (
                from d in _context.Details.Where(d => !d.isDeleted)
                join t in _context.Technologies.Where(t => !t.isDeleted)
                    on d.Id equals t.DetailId
                orderby d.Name
                select new
                {
                    TechnologyId = t.Id,
                    DetailName = d.Name
                }
            ).ToListAsync();

            model.OperationsList = new SelectList(operations, "Id", "Name", model.OperationId);
            model.DetailsList = new SelectList(detailsWithTech, "TechnologyId", "DetailName", model.TechnologyId);
        }
        [HttpGet]
        public async Task<IActionResult> Modify(int id)
        {
            var targetItem = await _context.TechnologyOperations.FirstOrDefaultAsync(item => item.Id == id);
            if (targetItem == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найден или удален." });
            }
            var model = new TechOpViewModel()
            {
                TechnologyId = targetItem.TechnologyId,
                OperationId = targetItem.OperationId
            };

            await RebuildSelectListsAsync(model);

            return PartialView("Views/TechOps/_ModifyPartial.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Modify(TechOpViewModel model)
        {
            if (model.TechnologyId <= 0)
            {
                ModelState.AddModelError("TechnologyId", "Необходимо выбрать деталь из списка.");
            }
            if (model.OperationId <= 0)
            {
                ModelState.AddModelError("OperationId", "Необходимо выбрать технологическую операцию из списка.");
            }
            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/TechOps/_ModifyPartial.cshtml", model);
            }
            var dbOperation = await _context.TechnologyOperations.FindAsync(model.Id);
            if (dbOperation == null)
            {
                return NotFound();
            }
            dbOperation.TechnologyId = model.TechnologyId;
            dbOperation.OperationId = model.OperationId;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var target = await _context.TechnologyOperations.FirstOrDefaultAsync(tech => tech.Id == id);
            if (target == null)
            {
                return NotFound(new { message = $"Объект с ID {id} не найден или уже удален." });
            }
            target.isDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
