using TestUmbraco.Application.Contracts;
using TestUmbraco.Application.DTO;
using TestUmbraco.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;
using reCAPTCHA.AspNetCore.Attributes;
using AspNetCoreHero.ToastNotification.Abstractions;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Controllers
{
    /// <summary>
    /// SurfaceController для обработки отправки форм конструктора форм
    /// </summary>
    public class FormBuilderController : SurfaceController
    {
        private readonly IEmailService _emailService;
        private readonly IFormSubmissionService _submissionService;
        private readonly INotyfService _notyfService;
        private readonly ILogger<FormBuilderController> _logger;

        /// <summary>
        /// Конструктор с dependency injection
        /// </summary>
        public FormBuilderController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IEmailService emailService,
            IFormSubmissionService submissionService,
            INotyfService notyfService,
            ILogger<FormBuilderController> logger)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _emailService = emailService;
            _submissionService = submissionService;
            _notyfService = notyfService;
            _logger = logger;
        }

        /// <summary>
        /// Обработка отправки формы
        /// </summary>
        /// <param name="model">Модель с данными формы</param>
        /// <returns>JSON результат или редирект</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForm(FormSubmissionModel model)
        {
            // Проверка, является ли запрос AJAX
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            try
            {
                _logger.LogInformation("=== FORM SUBMISSION STARTED ===");
                _logger.LogInformation("FormId: {FormId}", model.FormId);
                _logger.LogInformation("FormTitle: {FormTitle}", model.FormTitle);
                _logger.LogInformation("EmailRecipient: {EmailRecipient}", model.EmailRecipient);
                _logger.LogInformation("EmailSubject: {EmailSubject}", model.EmailSubject);
                _logger.LogInformation("SuccessMessage: {SuccessMessage}", model.SuccessMessage);
                _logger.LogInformation("Fields count: {Count}", model.Fields?.Count ?? 0);
                _logger.LogInformation("Is AJAX request: {IsAjax}", isAjax);
                
                // 1. Server-side валидация
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Form validation failed for form {FormId}. Errors: {Errors}", 
                        model.FormId, 
                        string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Проверьте правильность заполнения полей" });
                    }
                    
                    _notyfService.Error("Проверьте правильность заполнения полей");
                    return CurrentUmbracoPage();
                }

                // 2. Проверка reCAPTCHA токена (если передан)
                var recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
                if (string.IsNullOrEmpty(recaptchaToken))
                {
                    recaptchaToken = Request.Form["ReCaptchaToken"].ToString();
                }
                
                if (string.IsNullOrEmpty(recaptchaToken))
                {
                    _logger.LogWarning("reCAPTCHA token not provided for form {FormId}", model.FormId);
                    // Продолжаем без reCAPTCHA для тестирования
                    // TODO: Раскомментировать для production
                    // _notyfService.Error("Ошибка проверки reCAPTCHA");
                    // return CurrentUmbracoPage();
                }

                // 3. Получение настроек формы из Umbraco
                // Note: Form configuration should be passed from the view or accessed differently
                // since formBuilderBlock is a block element, not a content node
                // For now, we'll skip server-side pattern validation and rely on client-side validation
                // TODO: Implement proper server-side validation pattern handling
                
                // 4. Создание DTO
                var dto = new FormSubmissionDto
                {
                    FormId = model.FormId,
                    FormTitle = !string.IsNullOrEmpty(model.FormTitle) ? model.FormTitle : "Form Submission",
                    SubmittedAt = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    FieldValues = model.Fields ?? new Dictionary<string, string>(),
                    LogoUrl = model.LogoMailUrl
                };

                // 5. Сохранение в БД
                var saved = await _submissionService.ProcessSubmissionAsync(dto);
                if (!saved)
                {
                    _logger.LogWarning(
                        "Failed to save form submission to database for form {FormId}, but continuing with email send",
                        model.FormId);
                }

                // 6. Отправка email
                var emailRecipient = model.EmailRecipient;
                var emailSubject = !string.IsNullOrEmpty(model.EmailSubject) ? model.EmailSubject : "Новая отправка формы";

                _logger.LogInformation("Preparing to send email to: {EmailRecipient} with subject: {EmailSubject}", 
                    emailRecipient, emailSubject);

                if (string.IsNullOrWhiteSpace(emailRecipient))
                {
                    _logger.LogError("Email recipient not configured for form {FormId}", model.FormId);
                    
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Форма настроена некорректно. Email получатель не указан." });
                    }
                    
                    _notyfService.Error("Форма настроена некорректно. Email получатель не указан.");
                    return CurrentUmbracoPage();
                }

                _logger.LogInformation("Calling SendFormSubmissionAsync...");
                var emailSent = await _emailService.SendFormSubmissionAsync(dto, emailRecipient, emailSubject);
                _logger.LogInformation("SendFormSubmissionAsync returned: {EmailSent}", emailSent);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send email for form {FormId}", model.FormId);
                    
                    if (isAjax)
                    {
                        return Json(new { success = false, message = "Произошла ошибка при отправке формы. Попробуйте позже." });
                    }
                    
                    _notyfService.Error("Произошла ошибка при отправке формы. Попробуйте позже.");
                    return CurrentUmbracoPage();
                }

                // 7. Логирование успешной отправки
                _logger.LogInformation(
                    "Form {FormId} submitted successfully by IP {IpAddress}",
                    model.FormId,
                    dto.IpAddress);

                // 8. Возврат результата
                var successMessage = !string.IsNullOrEmpty(model.SuccessMessage) 
                    ? model.SuccessMessage 
                    : "Форма успешно отправлена!";
                
                if (isAjax)
                {
                    return Json(new { success = true, message = successMessage });
                }
                
                _notyfService.Success(successMessage);
                return RedirectToCurrentUmbracoPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing form {FormId}", model.FormId);
                
                if (isAjax)
                {
                    return Json(new { success = false, message = "Произошла системная ошибка. Попробуйте позже." });
                }
                
                _notyfService.Error("Произошла системная ошибка. Попробуйте позже.");
                return CurrentUmbracoPage();
            }
        }
    }
}
