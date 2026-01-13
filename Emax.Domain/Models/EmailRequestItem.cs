namespace Emax.Domain.Models
{
    /// <summary>
    /// База данных  
    /// Таблица EmailRequestItem - Форма отправки заявки
    /// </summary>
    public class EmailRequestItem : EntityBase
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}

