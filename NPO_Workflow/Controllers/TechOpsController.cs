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
        [Route("/Technologies/Operations/{id}", Name = "TechOperationsIndex")]
        public async Task<IActionResult> Index(int id, int? selectedOpId, int page = 1)
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = from teo in _context.TechnologyOperations.Where(t => !t.IsDeleted && t.TechnologyId == id)
                        join te in _context.Technologies.Where(d => !d.IsDeleted)
                            on teo.TechnologyId equals te.Id into techJoin
                        from tech in techJoin.DefaultIfEmpty()
                        join op in _context.Operations.Where(d => !d.IsDeleted)
                            on teo.OperationId equals op.Id into opJoin
                        from operation in opJoin.DefaultIfEmpty()
                        join de in _context.Details.Where(d => !d.IsDeleted)
                            on (tech != null ? tech.DetailId : 0) equals de.Id into detailJoin
                        from detail in detailJoin
                        .DefaultIfEmpty()
                        select new TechOpViewModel
                        {
                            Id = teo.Id,
                            HierarchyId = teo.HierarchyId,
                            TechnologyId = teo.TechnologyId,
                            OperationId = teo.OperationId,
                            OperationName = operation != null ? operation.Name : "-",
                            HourLength = operation.HourLength
                        };

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var items = await query
                .OrderByDynamic("HierarchyId", false)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var detailName = await (from tech in _context.Technologies.Where(item => !item.IsDeleted && item.Id == id)
                from detail in _context.Details.Where(d => !d.IsDeleted && tech.DetailId == d.Id)
                select detail.Name).FirstOrDefaultAsync() ?? "-";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.DetailName = detailName;
            ViewBag.SelectedOpId = selectedOpId;
            ViewBag.Columns = new List<SortColumn>
            {
                new() { Key = "HierarchyId", Label="№"},
                new() { Key = "OperationName", Label = "Наименование операции" },
                new() { Key = "HourLength", Label = "Время операции"}
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
        public async Task<IActionResult> Create([FromQuery] int techid, TechOpViewModel model)
        {
            if (model.OperationId <= 0)
            {
                ModelState.AddModelError("OperationId", "Необходимо выбрать операцию из списка.");
            }

            if (!ModelState.IsValid)
            {
                await RebuildSelectListsAsync(model);
                return PartialView("Views/TechOps/_CreatePartial.cshtml", model);
            }
            var lastItem = await _context.TechnologyOperations
                .Where(item => item.TechnologyId == techid)
                .OrderByDescending(item => item.HierarchyId) 
                .FirstOrDefaultAsync() ?? new TechnologyOperation() { HierarchyId = 0 };
            var newItem = new TechnologyOperation
            {
                HierarchyId = lastItem.HierarchyId + 1,
                TechnologyId = techid,
                OperationId = model.OperationId,  
                IsDeleted = false
            };
            _context.TechnologyOperations.Add(newItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        private async Task RebuildSelectListsAsync(TechOpViewModel model)
        {
            var operations = await _context.Operations
                .Where(op => !op.IsDeleted)
                .OrderBy(op => op.Name)
                .ToListAsync();
            var detailsWithTech = await (
                from d in _context.Details.Where(d => !d.IsDeleted)
                join t in _context.Technologies.Where(t => !t.IsDeleted)
                    on d.Id equals t.DetailId
                orderby d.Name
                select new
                {
                    TechnologyId = t.Id,
                    DetailName = d.Name
                }
            ).ToListAsync();

            model.OperationsList = new SelectList(operations, "Id", "Name", model.OperationId);
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
                HierarchyId = targetItem.HierarchyId,
                TechnologyId = targetItem.TechnologyId,
                OperationId = targetItem.OperationId
            };

            await RebuildSelectListsAsync(model);

            return PartialView("Views/TechOps/_ModifyPartial.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Modify(TechOpViewModel model)
        {
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
            target.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Move([FromQuery] int techopid, [FromQuery] int direction, [FromQuery] int? selectedOpId)
        {
            var currentOp = await _context.TechnologyOperations.FirstOrDefaultAsync(tech => tech.Id == techopid);
            if (currentOp == null) return NotFound();

            int techId = currentOp.TechnologyId;
            int currentHId = currentOp.HierarchyId;

            TechnologyOperation? neighborOp = null;

            if (direction == 1)
            {
                neighborOp = await _context.TechnologyOperations
                    .Where(tech => tech.TechnologyId == techId && tech.HierarchyId < currentHId && !tech.IsDeleted)
                    .OrderByDescending(tech => tech.HierarchyId).FirstOrDefaultAsync();
            }
            else if (direction == 0)
            {
                neighborOp = await _context.TechnologyOperations
                    .Where(tech => tech.TechnologyId == techId && tech.HierarchyId > currentHId && !tech.IsDeleted)
                    .OrderBy(tech => tech.HierarchyId).FirstOrDefaultAsync();
            }

            if (neighborOp != null)
            {
                (currentOp.HierarchyId, neighborOp.HierarchyId) = (neighborOp.HierarchyId, currentOp.HierarchyId);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Operations", "Technologies", new { id = techId, selectedOpId = selectedOpId });
        }
    }
}
