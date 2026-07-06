using System.ComponentModel.DataAnnotations;
namespace NPO_Workflow.DAL.Models;

public class CalendarWeek
{
    [Key]
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public decimal WorkingHours { get; set; }
}