using TestUmbraco.Application.DTO;
using TestUmbraco.Domain.Models;

namespace TestUmbraco.Application.Contracts
{
    /// <summary>
    /// Сервис для обработки отправок форм конструктора форм
    /// </summary>
    public interface IFormSubmissionService
    {
        /// <summary>
        /// Обработать отправку формы (сохранить в БД)
        /// </summary>
        /// <param name="dto">DTO с данными отправки формы</param>
        /// <returns>True если сохранение успешно, False в случае ошибки</returns>
        Task<bool> ProcessSubmissionAsync(FormSubmissionDto dto);

        /// <summary>
        /// Получить все отправки конкретной формы
        /// </summary>
        /// <param name="formId">ID формы (formBuilderBlock)</param>
        /// <returns>Коллекция отправок формы</returns>
        Task<IEnumerable<FormSubmission>> GetSubmissionsByFormAsync(int formId);
    }
}
