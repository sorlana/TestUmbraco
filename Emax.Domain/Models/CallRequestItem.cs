namespace Emax.Domain.Models
{
    /// <summary>
    /// База данных  
    /// Таблица CallRequestItem - Обратный звонок
    /// </summary>
    public class CallRequestItem : EntityBase
    {
        public string Phone { get; set; }
        public string TimeCall { get; set; }
    }

}

