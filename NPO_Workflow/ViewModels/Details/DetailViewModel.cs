using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NPO_Workflow.ViewModels.Details
{
    public class DetailViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование детали обязательно для заполнения")]
        [MaxLength(200, ErrorMessage = "Наименование детали не может превышать 200 символов")]
        [Display(Name = "Наименование детали")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Родительская деталь")]
        public int? ParentId { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать заказ")]
        [Display(Name = "Заказ")]
        public int? OrderId { get; set; }

        [Required(ErrorMessage = "Количество деталей должно быть указано")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество деталей должно быть не меньше 1")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Заказ")]
        public string? OrderName { get; set; }

        [Display(Name = "Родительская деталь")]
        public string? ParentDetailName { get; set; }
        public SelectList? OrdersList { get; set; }
        public SelectList? ParentDetailsList { get; set; }
    }
}