using System.ComponentModel.DataAnnotations;
namespace NPO_Workflow.DAL.Models;

public class CalendarException
{
    [Key]
    public int Id { get; set; }
    public int CalendarId { get; set; }
    public DateOnly Date { get; set; }
    public decimal WorkingHours { get; set; }
    public string? Comment { get; set; }
    public bool IsDeleted { get; set; } = false;
}