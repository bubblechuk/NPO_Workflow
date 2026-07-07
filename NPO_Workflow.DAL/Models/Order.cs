using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.DAL.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование заказа обязательно для заполнения")]
        [MaxLength(150, ErrorMessage = "Наименование заказа не может превышать 150 символов")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Международное наименование не может превышать 150 символов")]
        public string? InternationalName { get; set; }

        [MaxLength(500, ErrorMessage = "Комментарий не может превышать 500 символов")]
        public string? Comment { get; set; }
        public bool IsDeleted { get; set; }
    }
}