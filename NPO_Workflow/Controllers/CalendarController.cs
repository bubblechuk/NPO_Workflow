using Microsoft.AspNetCore.Mvc;
using NPO_Workflow.DAL;
using NPO_Workflow.ViewModels.Calendar;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NPO_Workflow.DAL.Models;

namespace NPO_Workflow.Controllers;


public class CalendarController : Controller
{
    private readonly NPOContext _context;
    [HttpGet("Calendar")]
    [HttpGet("Calendar/{year:int}/{month:int}")]
    public async Task<IActionResult> Index(int? year, int? month)
    {
        if (year == null || month == null)
        {
            year = DateTime.Now.Year;
            month = DateTime.Now.Month;
        }
        if (year is < 2000 or > 2099 ||  month is < 1 or > 12) 
            return BadRequest(new {message = "Некорректный год или месяц!"});
        var firstDay = new DateOnly(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        int dayOfWeekValue = ((int)firstDay.DayOfWeek == 0) ? 7 : (int)firstDay.DayOfWeek;
        int emptyDaysBefore = dayOfWeekValue - 1;
        var calendarWeeks = await _context.CalendarWeeks
            .Where(t => t.CalendarId == 1)
            .ToDictionaryAsync(t => t.DayOfWeek, t => t.WorkingHours);
        var calendarExceptions = await _context.CalendarExceptions
            .Where(e => e.CalendarId == 1 && e.Date >= firstDay && e.Date <= lastDay && e.IsDeleted == false)
            .ToDictionaryAsync(e => e.Date, e => e);
        var daysList = new List<CalendarDayItemDto>();
        for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
        {
            decimal hours = 0;
            string? comment = string.Empty;
            bool isException = false;

            if (calendarExceptions.TryGetValue(date, out var exception))
            {
                hours = exception.WorkingHours;
                comment = exception.Comment;
                isException = true;
            }
            else if (calendarWeeks.TryGetValue(date.DayOfWeek, out var templateHours))
            {
                hours = templateHours;
            }
            daysList.Add(new CalendarDayItemDto
            {
                Date = date,
                WorkingHours = hours,
                Comment = comment,
                IsException = isException
            });
        }

        var nextMonth = firstDay.AddMonths(1);
        var prevMonth = firstDay.AddMonths(-1);
        var viewModel = new CalendarViewModel()
        {
            Year = year,
            Month = month,
            MonthName = firstDay.ToString("MMMM", new CultureInfo("ru-RU")).ToUpper(),
            NextMonthName = nextMonth.ToString("MMMM", new CultureInfo("ru-RU")).ToUpper(),
            PreviousMonthName = prevMonth.ToString("MMMM", new CultureInfo("ru-RU")).ToUpper(),
            PrevYear = firstDay.AddYears(-1).Year,
            NextMonth = nextMonth.Month,
            PrevMonth =  prevMonth.Month,
            NextYear = firstDay.AddYears(1).Year,
            EmptyDaysBefore = emptyDaysBefore,
            Days = daysList
        };
        return View(viewModel);
    }

    [HttpGet("Calendar/Create/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Create(int year, int month, int day)
    {
        var viewModel = new CalendarDayItemDto
        {
            Date = new DateOnly(year, month, day)
        };
        return PartialView("/Views/Calendar/_CreatePartial.cshtml", viewModel);
    }
    [HttpPost("Calendar/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CalendarDayItemDto viewModel)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("/Views/Calendar/_CreatePartial.cshtml", viewModel);
        }
        var calendarException = new CalendarException
        {
            CalendarId = 1,
            Date = viewModel.Date,
            WorkingHours = viewModel.WorkingHours,
            Comment = viewModel.Comment
        };
        _context.CalendarExceptions.Add(calendarException);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    [HttpGet("/Calendar/Modify/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Modify(int year, int month, int day)
    {
        var targetDate = new DateOnly(year, month, day);
        var targetItem = await _context.CalendarExceptions.FirstOrDefaultAsync(ce => ce.Date == targetDate && ce.IsDeleted == false);
        if (targetItem == null)
        {
            return NotFound(new { message = $"Объект с датой {targetDate.ToString()} не найден или уже удален." });
        }

        var viewModel = new CalendarDayItemDto
        {
            Date = targetItem.Date,
            WorkingHours = targetItem.WorkingHours,
            Comment = targetItem.Comment
        };
        return PartialView("/Views/Calendar/_ModifyPartial.cshtml", viewModel);
    }

    [HttpPost("/Calendar/Modify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Modify(CalendarDayItemDto viewModel)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("/Views/Calendar/_ModifyPartial.cshtml", viewModel);
        }
        var dbOperation = await _context.CalendarExceptions.FirstOrDefaultAsync(ce => ce.Date == viewModel.Date && ce.IsDeleted == false);
        var duplicate = await _context.CalendarExceptions.FirstOrDefaultAsync(ce => ce.Date == viewModel.Date);
        if (dbOperation == null)
        {
            return NotFound();
        }

        if (duplicate == null)
        {
            return BadRequest();
        }
        dbOperation.WorkingHours = viewModel.WorkingHours;
        dbOperation.Comment = viewModel.Comment;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("/Calendar/Delete/{year:int}/{month:int}/{day:int}")]
    public async Task<IActionResult> Delete(int year, int month, int day)
    {
        var targetDate = new DateOnly(year, month, day);
        var target = await _context.CalendarExceptions.FirstOrDefaultAsync(ce => ce.Date == targetDate &&  ce.IsDeleted == false );
        if (target == null)
        {
            return NotFound(new { message = $"Объект с датой {targetDate.ToString()} не найден или уже удален." });
        }
        target.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    public CalendarController(NPOContext context)
    {
        _context = context;
    }
}