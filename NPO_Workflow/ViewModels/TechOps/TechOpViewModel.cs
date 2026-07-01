using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.ViewModels.TechOps
{
    public class TechOpViewModel
    {
        public int Id { get; set; }
        public int HierarchyId { get; set; }
        public int TechnologyId { get; set; }
        public int OperationId { get; set; }
        public string? OperationName { get; set; }
        public SelectList? OperationsList { get; set; }
    }
}
