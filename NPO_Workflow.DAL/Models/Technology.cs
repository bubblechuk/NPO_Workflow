using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.DAL.Models
{
    public class Technology
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DetailId { get; set; }
    }
}