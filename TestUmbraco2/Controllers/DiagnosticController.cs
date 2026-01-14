using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace TestUmbraco.Controllers
{
    [Route("api/diagnostic")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;
        private readonly ITemplateService _templateService;

        public DiagnosticController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IContentService contentService,
            ITemplateService templateService)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService;
            _templateService = templateService;
        }

        // Простейшая проверка статуса
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Diagnostic API работает",
                timestamp = DateTime.UtcNow,
                umbracoVersion = "17.1.0"
            });
        }

        // Получаем корневые элементы только через IContentService
        [HttpGet("root")]
        public IActionResult GetRootInfo()
        {
            try
            {
                // Используем только IContentService (работает гарантированно)
                var rootContent = _contentService.GetRootContent()?.ToList() ?? new List<IContent>();
                
                // Пытаемся проверить через UmbracoContext
                var hasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext);
                
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    
                    // Корневые элементы из базы данных (через IContentService)
                    contentServiceRootCount = rootContent.Count,
                    contentServiceRoots = rootContent.Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        published = r.Published,
                        templateId = r.TemplateId,
                        contentType = r.ContentType.Alias,
                        hasTemplate = r.TemplateId > 0
                    }).ToList(),
                    
                    // Дополнительная информация о состоянии
                    umbracoContextAvailable = hasUmbracoContext,
                    firstRootId = rootContent.FirstOrDefault()?.Id,
                    
                    // Рекомендации
                    recommendations = new[]
                    {
                        "1. Убедитесь, что в системе есть хотя бы один корневой документ",
                        "2. Проверьте, что документ опубликован",
                        "3. Убедитесь, что документу назначен шаблон",
                        "4. Проверьте наличие файла шаблона в папке Views/"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        // Проверка конкретного контента по ID
        [HttpGet("content/{id:int}")]
        public IActionResult GetContent(int id)
        {
            try
            {
                var content = _contentService.GetById(id);
                if (content == null)
                {
                    return NotFound(new { error = $"Content with ID {id} not found" });
                }

                // Получаем информацию о шаблоне
                string templateName = "No template";
                if (content.TemplateId > 0)
                {
                    var template = _templateService.GetAsync(content.TemplateId.Value).GetAwaiter().GetResult();
                    templateName = template?.Name ?? "Unknown";
                }

                return Ok(new
                {
                    id = content.Id,
                    name = content.Name,
                    published = content.Published,
                    templateId = content.TemplateId,
                    templateName = templateName,
                    contentType = content.ContentType.Alias,
                    parentId = content.ParentId,
                    path = content.Path
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Простая проверка шаблонов
        [HttpGet("templates")]
        public IActionResult GetTemplates()
        {
            try
            {
                var templates = _templateService.GetAllAsync().GetAwaiter().GetResult();
                
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    templatesCount = templates.Count(),
                    templates = templates.Select(t => new
                    {
                        id = t.Id,
                        name = t.Name,
                        alias = t.Alias
                    }).ToList(),
                    notes = new[]
                    {
                        "Убедитесь, что для каждого шаблона существует файл в папке Views/",
                        "Имя файла должно точно соответствовать имени шаблона (с учетом пробелов)",
                        "Пример: шаблон 'Block Grid' требует файл 'Block Grid.cshtml'"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = ex.Message, 
                    note = "Ошибка при получении списка шаблонов" 
                });
            }
        }

        // Проверка проблемы с шаблоном BlockGrid
        [HttpGet("check-blockgrid")]
        public IActionResult CheckBlockGrid()
        {
            try
            {
                var templates = _templateService.GetAllAsync().GetAwaiter().GetResult();
                var blockGridTemplate = templates.FirstOrDefault(t => 
                    t.Name.Contains("Block") || t.Alias.Contains("BlockGrid"));
                
                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    blockGridTemplateExists = blockGridTemplate != null,
                    templateInfo = blockGridTemplate != null ? new
                    {
                        id = blockGridTemplate.Id,
                        name = blockGridTemplate.Name,
                        alias = blockGridTemplate.Alias
                    } : null,
                    advice = blockGridTemplate != null 
                        ? $"Найден шаблон '{blockGridTemplate.Name}'. Убедитесь, что файл '{blockGridTemplate.Name}.cshtml' существует в папке Views/"
                        : "Шаблон BlockGrid не найден в системе"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}