using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.DAL.Models
{
    public class Detail
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Наименование детали обязательно для заполнения")]
        [MaxLength(200, ErrorMessage = "Наименование детали не может превышать 200 символов")]
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        [Required]
        public int OrderId { get; set; }
        [Required(ErrorMessage = "Количество деталей должно быть указано")]
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; }
    }
}