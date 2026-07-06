using NPO_Workflow.DAL.Models;

namespace NPO_Workflow.ViewModels.Calendar;

public class CalendarViewModel
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public string MonthName { get; set; } = String.Empty;
    public string PreviousMonthName { get; set; } = String.Empty;
    public string NextMonthName { get; set; } = String.Empty;
    public int PrevMonth { get; set; }
    public int PrevYear { get; set; }
    public int NextMonth { get; set; }
    public int NextYear { get; set; }
    public int EmptyDaysBefore { get; set; }
    public List<CalendarDayItemDto> Days { get; set; } = new();
}
public class CalendarDayItemDto
{
    public DateOnly Date { get; set; }
    public decimal WorkingHours { get; set; }
    public string Comment { get; set; }
    public bool IsException { get; set; }
    public bool IsWeekend => Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}