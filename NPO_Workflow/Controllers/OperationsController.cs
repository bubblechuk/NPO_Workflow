using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NPO_Workflow.DAL;
using NPO_Workflow.DAL.Models;
using NPO_Workflow.Extensions;
using NPO_Workflow.ViewModels;
using NPO_Workflow.ViewModels.Operations;

namespace NPO_Workflow.Controllers
{
    [Authorize(Roles = @"DESKTOP-RDH7AFR\docker-users")]
    public class OperationsController : Controller
    {
        private NPOContext _context;
        public async Task<IActionResult> Index(string search, int page = 1, string sortBy = "Id", bool? sortOrder = null)
        {
            int pageSize = 10;
            if (page < 1) page = 1;
            var query = _context.Operations.Where(operation => operation.isDeleted == false);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(op => op.Name.ToLower().Contains(searchLower)
                                       || op.Instruction.ToLower().Contains(searchLower)
                                       || op.Section.ToLower().Contains(searchLower));
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;

            var operations = await query
                .OrderByDynamic(sortBy, sortOrder ?? false)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(op => new OperationViewModel
                {
                    Id = op.Id,
                    Name = op.Name,
                    Instruction = op.Instruction,
                    Tpz = op.Tpz,
                    PaymentType = op.PaymentType,
                    Section = op.Section,
                    HourLength = op.HourLength
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
                new() { Key = "Instruction", Label = "№ Инструкции по ТБ" },
                new() { Key = "Tpz", Label = "Т п.з." },
                new() { Key = "PaymentType", Label = "Вид опл." },
                new() { Key = "Section", Label = "Участок" },
                new() { Key = "HourLength", Label = "Опл. (ч)" }
            };
            return View(operations);
        }
        public OperationsController(NPOContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("Views/Operations/_CreatePartial.cshtml", new CreateOperationViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOperationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Views/Operations/_CreatePartial.cshtml", model);
            }
            var newOperation = new Operation
            {
                Name = model.Name,
                Instruction = model.Instruction,
                Tpz = model.Tpz ?? 0,
                PaymentType = model.PaymentType,
                Section = model.Section,
                HourLength = model.HourLength ?? 0,
                isDeleted = false
            };
            _context.Operations.Add(newOperation);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Modify(int id)
        {
            var targetOperation = await _context.Operations.FirstOrDefaultAsync(operation => operation.Id == id);
            if (targetOperation == null)
            {
                return NotFound(new { message = $"Операция с ID {id} не найдена или удалена." });
            }
            var model = new ModifyOperationViewModel()
            {
                Id = targetOperation.Id,
                Name = targetOperation.Name,
                Instruction = targetOperation.Instruction,
                Tpz = targetOperation.Tpz,
                PaymentType = targetOperation.PaymentType,
                Section = targetOperation.Section,
                HourLength = targetOperation.HourLength
            };
            return PartialView("Views/Operations/_ModifyPartial.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> Modify(ModifyOperationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Views/Operations/_ModifyPartial.cshtml", model);
            }
            var dbOperation = await _context.Operations.FindAsync(model.Id);
            if (dbOperation == null)
            {
                return NotFound();
            }
            dbOperation.Name = model.Name;
            dbOperation.Instruction = model.Instruction;
            dbOperation.Tpz = model.Tpz ?? 0;
            dbOperation.PaymentType = model.PaymentType;
            dbOperation.Section = model.Section;
            dbOperation.HourLength = model.HourLength ?? 0;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var targetOperation = await _context.Operations.FirstOrDefaultAsync(operation => operation.Id == id);
            if (targetOperation == null) {
                return NotFound(new { message = $"Операция с ID {id} не найдена или уже удалена." });
            }
            targetOperation.isDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
