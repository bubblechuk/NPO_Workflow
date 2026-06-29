using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.ViewModels.Technologies
{
    public class TechnologyViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Наименование детали")]
        public int DetailId { get; set; }
        public string? DetailName { get; set; }
        public SelectList? DetailsList { get; set; }
    }
}
