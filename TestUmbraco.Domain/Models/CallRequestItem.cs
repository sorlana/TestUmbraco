using System.ComponentModel.DataAnnotations;

namespace TestUmbraco.Domain.Models
{
    /// <summary>
    /// База данных  
    /// Таблица CallRequestItem - Обратный звонок
    /// </summary>
    public class CallRequestItem : EntityBase
    {
        [Required(ErrorMessage = "Телефон обязателен для заполнения")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Время звонка обязательно для заполнения")]
        public string TimeCall { get; set; } = string.Empty;
    }
}