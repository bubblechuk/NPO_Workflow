using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.ViewModels.Operations
{
    public class CreateOperationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование обязательно для заполнения")]
        public string Name { get; set; } = string.Empty;
        public string? Instruction { get; set; }

        [Required(ErrorMessage = "Норма Т п.з. обязательна")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Значение Т п.з. не может быть отрицательным")]
        public float? Tpz { get; set; }

        public string? PaymentType { get; set; }

        [Required(ErrorMessage = "Участок обязателен для заполнения")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Длительность операции обязательна")]
        [Range(0, int.MaxValue, ErrorMessage = "Длительность не может быть отрицательной")]
        public int? HourLength { get; set; }
    }
}
