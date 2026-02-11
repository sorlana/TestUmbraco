using TestUmbraco.Application.Contracts;
using TestUmbraco.Application.DTO;
using TestUmbraco.Domain.Contracts;
using TestUmbraco.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TestUmbraco.Application.Services
{
    /// <summary>
    /// Сервис для обработки отправок форм конструктора форм
    /// </summary>
    public class FormSubmissionService : IFormSubmissionService
    {
        private readonly IFormSubmissionRepository _repository;
        private readonly ILogger<FormSubmissionService> _logger;

        public FormSubmissionService(
            IFormSubmissionRepository repository,
            ILogger<FormSubmissionService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Обработать отправку формы (сохранить в БД)
        /// </summary>
        /// <param name="dto">DTO с данными отправки формы</param>
        /// <returns>True если сохранение успешно, False в случае ошибки</returns>
        public async Task<bool> ProcessSubmissionAsync(FormSubmissionDto dto)
        {
            if (dto == null)
            {
                _logger.LogError("FormSubmissionDto is null");
                return false;
            }

            try
            {
                // Преобразовать FormSubmissionDto в FormSubmission entity
                var entity = new FormSubmission
                {
                    FormId = dto.FormId,
                    FormTitle = dto.FormTitle ?? string.Empty,
                    SubmittedAt = dto.SubmittedAt,
                    IpAddress = dto.IpAddress ?? string.Empty,
                    // Сериализовать FieldValues в JSON
                    FieldValuesJson = JsonSerializer.Serialize(dto.FieldValues)
                };

                // Сохранить через IFormSubmissionRepository
                var id = await _repository.SaveAsync(entity);

                _logger.LogInformation(
                    "Form submission saved successfully. FormId: {FormId}, FormTitle: {FormTitle}, SubmissionId: {SubmissionId}",
                    dto.FormId,
                    dto.FormTitle,
                    id);

                // Вернуть true при успехе
                return true;
            }
            catch (Exception ex)
            {
                // Вернуть false при ошибке (с логированием)
                _logger.LogError(
                    ex,
                    "Ошибка при сохранении отправки формы. FormId: {FormId}, FormTitle: {FormTitle}, Error: {Message}",
                    dto.FormId,
                    dto.FormTitle,
                    ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Получить все отправки конкретной формы
        /// </summary>
        /// <param name="formId">ID формы (formBuilderBlock)</param>
        /// <returns>Коллекция отправок формы</returns>
        public async Task<IEnumerable<FormSubmission>> GetSubmissionsByFormAsync(int formId)
        {
            try
            {
                // Получить отправки через IFormSubmissionRepository
                var submissions = await _repository.GetByFormIdAsync(formId);

                _logger.LogInformation(
                    "Retrieved {Count} submissions for form {FormId}",
                    submissions.Count(),
                    formId);

                return submissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при получении отправок формы. FormId: {FormId}, Error: {Message}",
                    formId,
                    ex.Message);

                // Возвращаем пустую коллекцию в случае ошибки
                return Enumerable.Empty<FormSubmission>();
            }
        }
    }
}
