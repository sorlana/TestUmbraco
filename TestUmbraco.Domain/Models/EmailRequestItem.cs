using System.ComponentModel.DataAnnotations;

namespace TestUmbraco.Domain.Models
{
    /// <summary>
    /// База данных  
    /// Таблица EmailRequestItem - Форма отправки заявки
    /// </summary>
    public class EmailRequestItem : EntityBase
    {
        [Required(ErrorMessage = "Имя обязательно для заполнения")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Телефон обязателен для заполнения")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Комментарий обязателен для заполнения")]
        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
        public string Comment { get; set; } = string.Empty;
    } 
}
