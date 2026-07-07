using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.ViewModels.Technologies
{
    public class TechnologyViewModel : IValidatableObject
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Наименование детали")]
        public int DetailId { get; set; }
        public string? DetailName { get; set; }
        public DateOnly? BeginDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public SelectList? DetailsList { get; set; }
        public decimal? HourLength { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (BeginDate.HasValue && BeginDate.Value < today)
            {
                yield return new ValidationResult(
                    "Дата начала не может быть в прошлом.",
                    new[] { nameof(BeginDate) }
                );
            }
            if (EndDate.HasValue && EndDate.Value < today)
            {
                yield return new ValidationResult(
                    "Дата конца не может быть в прошлом.",
                    new[] { nameof(EndDate) }
                );
            }
            if (BeginDate.HasValue && EndDate.HasValue && EndDate.Value < BeginDate.Value)
            {
                yield return new ValidationResult(
                    "Дата окончания не может быть раньше даты начала.",
                    new[] { nameof(EndDate) }
                );
            }
        }
    }
}
