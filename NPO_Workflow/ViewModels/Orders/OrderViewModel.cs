using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.ViewModels.Orders
{
    public class OrderViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование заказа обязательно для заполнения")]
        [MaxLength(150, ErrorMessage = "Наименование заказа не может превышать 150 символов")]
        [MinLength(3, ErrorMessage = "Наименование заказа должно содержать минимум 3 символа")]
        [Display(Name = "Наименование заказа")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Международное наименование не может превышать 150 символов")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\/\.,_()]*$", ErrorMessage = "Международное наименование должно содержать только латинские буквы, цифры и базовые знаки препинания")]
        [Display(Name = "Международное наименование")]
        public string? InternationalName { get; set; }

        [MaxLength(500, ErrorMessage = "Комментарий не может превышать 500 символов")]
        [Display(Name = "Комментарий к заказу")]
        public string? Comment { get; set; }
    }
}