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

            var query = from te in _context.Technologies.Where(t => !t.IsDeleted)
                        join de in _context.Details.Where(d => !d.IsDeleted) on te.DetailId equals de.Id
                        select new TechnologyViewModel
                        {
                            Id = te.Id,
                            DetailId = te.DetailId,
                            DetailName = de.Name,
                            BeginDate = te.BeginDate,
                            EndDate = te.EndDate,
                            HourLength = (from to in _context.TechnologyOperations.Where(t => !t.IsDeleted) where to.TechnologyId == te.Id
                                join op in _context.Operations.Where(d => !d.IsDeleted) on to.OperationId equals op.Id
                                select op.HourLength).Sum()
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(vm => vm.DetailName != null && vm.DetailName.ToLower().Contains(searchLower));
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
                new() { Key = "Id", Label = "ID технологии" },
                new() { Key = "DetailId", Label = "Наименование детали" },
                new() { Key = "BeginDate", Label = "Дата начала"},
                new() { Key ="EndDate", Label="Дата конца"},
                new() { Key = "HourLength", Label="Время на выполнение"}
            };

            return View(items);
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
                .AnyAsync(t => !t.IsDeleted && t.DetailId == model.DetailId);
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
                BeginDate = model.BeginDate,
                EndDate = model.EndDate,
                IsDeleted = false
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
                DetailId = targetItem.DetailId,
                BeginDate = targetItem.BeginDate,
                EndDate = targetItem.EndDate,
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
            var currentHourLength = await (from to in _context.TechnologyOperations.Where(t => !t.IsDeleted) 
                where to.TechnologyId == model.Id
                join op in _context.Operations.Where(d => !d.IsDeleted) on to.OperationId equals op.Id
                select op.HourLength).SumAsync();
            if ( !(await IsEnoughTime(model.BeginDate, model.EndDate, currentHourLength)))
            {
                return BadRequest("Недостаточно времени на операцию");
            }

            var dbOperation = await _context.Technologies.FindAsync(model.Id);
            if (dbOperation == null)
            {
                return NotFound();
            }

            dbOperation.DetailId = model.DetailId;
            dbOperation.BeginDate = model.BeginDate;
            dbOperation.EndDate = model.EndDate;
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
            target.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task RebuildSelectListsAsync(TechnologyViewModel model)
        {
            var busyDetailIds = await _context.Technologies
                .Where(t => !t.IsDeleted)
                .Select(t => t.DetailId)
                .ToListAsync();
            var details = await _context.Details
                .Where(d => !d.IsDeleted && (!busyDetailIds.Contains(d.Id) || d.Id == model.DetailId))
                .ToListAsync();

            model.DetailsList = new SelectList(details, "Id", "Name", model.DetailId);
        }

        private async Task<bool> IsEnoughTime(DateOnly? startDate, DateOnly? endDate, decimal? hourLength)
        {
            if (!startDate.HasValue || !endDate.HasValue || !hourLength.HasValue || endDate.Value < startDate.Value)
            {
                return false;
            }
            var exceptions = await _context.CalendarExceptions
                .Where(e => !e.IsDeleted && e.Date >= startDate.Value && e.Date <= endDate.Value)
                .ToDictionaryAsync(e => e.Date, e => e.WorkingHours); 

            var calendarWeeks = await _context.CalendarWeeks
                .Where(w => w.CalendarId == 1)
                .ToDictionaryAsync(w => w.DayOfWeek, w => w.WorkingHours);

            decimal totalHours = 0m;
            var currentDate = startDate.Value;
            while (currentDate <= endDate.Value)
            {
                if (exceptions.TryGetValue(currentDate, out var exceptionHours))
                {
                    totalHours += exceptionHours;
                }
                else if (calendarWeeks.TryGetValue(currentDate.DayOfWeek, out var weekHours))
                {
                    totalHours += weekHours;
                }
                currentDate = currentDate.AddDays(1);
            }

            return totalHours >= hourLength.Value;
        }
    }
}