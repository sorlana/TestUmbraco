using System.ComponentModel.DataAnnotations;

namespace TestUmbraco.Domain.Models
{
    /// <summary>
    /// База данных  
    /// Таблица FormSubmission - Отправка формы конструктора форм
    /// </summary>
    public class FormSubmission : EntityBase
    {
        /// <summary>
        /// ID формы (formBuilderBlock)
        /// </summary>
        [Required]
        public int FormId { get; set; }
        
        /// <summary>
        /// Название формы
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FormTitle { get; set; } = string.Empty;
        
        /// <summary>
        /// Дата и время отправки
        /// </summary>
        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// IP адрес отправителя
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// Данные всех полей в JSON формате
        /// </summary>
        [Required]
        public string FieldValuesJson { get; set; } = string.Empty;
    }
}
