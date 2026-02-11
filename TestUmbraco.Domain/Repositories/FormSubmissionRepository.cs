using Microsoft.EntityFrameworkCore;
using TestUmbraco.Domain.Contracts;
using TestUmbraco.Domain.Models;

namespace TestUmbraco.Domain.Repositories
{
    /// <summary>
    /// Репозиторий для работы с отправками форм
    /// Использует Entity Framework Core и AppDbContext
    /// </summary>
    public class FormSubmissionRepository : IFormSubmissionRepository
    {
        private readonly AppDbContext _dbContext;

        public FormSubmissionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Сохранить отправку формы в базу данных
        /// </summary>
        /// <param name="submission">Отправка формы для сохранения</param>
        /// <returns>ID созданной записи</returns>
        public async Task<int> SaveAsync(FormSubmission submission)
        {
            await _dbContext.FormSubmissions.AddAsync(submission);
            await _dbContext.SaveChangesAsync();
            return submission.Id;
        }

        /// <summary>
        /// Получить отправку формы по ID
        /// </summary>
        /// <param name="id">ID отправки</param>
        /// <returns>Отправка формы или null, если не найдена</returns>
        public async Task<FormSubmission?> GetByIdAsync(int id)
        {
            return await _dbContext.FormSubmissions
                .FirstOrDefaultAsync(fs => fs.Id == id);
        }

        /// <summary>
        /// Получить все отправки конкретной формы
        /// </summary>
        /// <param name="formId">ID формы (formBuilderBlock)</param>
        /// <returns>Коллекция отправок формы</returns>
        public async Task<IEnumerable<FormSubmission>> GetByFormIdAsync(int formId)
        {
            return await _dbContext.FormSubmissions
                .Where(fs => fs.FormId == formId)
                .OrderByDescending(fs => fs.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Получить все отправки с пагинацией
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начиная с 1)</param>
        /// <param name="pageSize">Количество записей на странице</param>
        /// <returns>Коллекция отправок формы</returns>
        public async Task<IEnumerable<FormSubmission>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _dbContext.FormSubmissions
                .OrderByDescending(fs => fs.SubmittedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
