using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.DAL.Models
{
    public class Operation
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование операции обязательно для заполнения")]
        [MaxLength(250, ErrorMessage = "Наименование операции не может превышать 250 символов")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Инструкция не может превышать 1000 символов")]
        public string? Instruction { get; set; }

        [Required(ErrorMessage = "Значение Тп.з. обязательно")]
        public float Tpz { get; set; }

        public int? PaymentType { get; set; }

        [Required(ErrorMessage = "Участок обязателен для заполнения")]
        [MaxLength(100, ErrorMessage = "Название участка не может превышать 100 символов")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Длительность операции обязательна")]
        public decimal HourLength { get; set; }

        public bool IsDeleted { get; set; }
    }
}