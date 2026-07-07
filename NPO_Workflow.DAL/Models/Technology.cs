using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.DAL.Models
{
    public class Technology
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int DetailId { get; set; }
        public DateOnly? BeginDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}