using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.DAL.Models
{
    public class TechnologyOperation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int TechnologyId { get; set; }
        [Required]
        public int HierarchyId { get; set; }
        [Required]
        public int OperationId { get; set; }
        public bool isDeleted { get; set; }
    }
}