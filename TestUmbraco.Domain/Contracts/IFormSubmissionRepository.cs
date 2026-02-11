using TestUmbraco.Domain.Models;

namespace TestUmbraco.Domain.Contracts
{
    /// <summary>
    /// Репозиторий для работы с отправками форм
    /// </summary>
    public interface IFormSubmissionRepository
    {
        /// <summary>
        /// Сохранить отправку формы в базу данных
        /// </summary>
        /// <param name="submission">Отправка формы для сохранения</param>
        /// <returns>ID созданной записи</returns>
        Task<int> SaveAsync(FormSubmission submission);
        
        /// <summary>
        /// Получить отправку формы по ID
        /// </summary>
        /// <param name="id">ID отправки</param>
        /// <returns>Отправка формы или null, если не найдена</returns>
        Task<FormSubmission?> GetByIdAsync(int id);
        
        /// <summary>
        /// Получить все отправки конкретной формы
        /// </summary>
        /// <param name="formId">ID формы (formBuilderBlock)</param>
        /// <returns>Коллекция отправок формы</returns>
        Task<IEnumerable<FormSubmission>> GetByFormIdAsync(int formId);
        
        /// <summary>
        /// Получить все отправки с пагинацией
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начиная с 1)</param>
        /// <param name="pageSize">Количество записей на странице</param>
        /// <returns>Коллекция отправок формы</returns>
        Task<IEnumerable<FormSubmission>> GetAllAsync(int pageNumber, int pageSize);
    }
}
