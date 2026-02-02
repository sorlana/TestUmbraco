using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;
using System.Collections.Concurrent;
using Umbraco.Extensions;

namespace TestUmbraco.Services
{
    public interface IStaticCssGeneratorService
    {
        Task<string> GenerateBackgroundCssFileAsync();
        Task UpdateCssForMediaAsync(Guid mediaKey);
        Task RemoveCssForMediaAsync(Guid mediaKey);
        Task RegenerateAllCssAsync();
        Task<string> AddInlineStyleAsync(string css, string styleType = "custom");
        Task<string> GetOrAddMediaClassAsync(Guid mediaKey, string className, int minHeight = 400, string size = "cover", string position = "center");
        Task<string> GetOrAddColorClassAsync(string colorValue, int minHeight = 400);
        Task<string> GetOrAddGradientClassAsync(string colorStart, string colorEnd, string direction = "to bottom", int minHeight = 400);
        Task<string> AddOverlayStyleAsync(string overlayClass, string css);
    }

    public class StaticCssGeneratorService : IStaticCssGeneratorService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IMediaService _mediaService;
        private readonly IMediaCacheService _mediaCacheService;
        private readonly ILogger<StaticCssGeneratorService> _logger;
        private readonly string _cssFilePath;
        private readonly ConcurrentDictionary<string, string> _styleCache = new();
        private readonly object _fileLock = new();

        public StaticCssGeneratorService(
            IWebHostEnvironment env,
            IMediaService mediaService,
            IMediaCacheService mediaCacheService,
            ILogger<StaticCssGeneratorService> logger)
        {
            _env = env;
            _mediaService = mediaService;
            _mediaCacheService = mediaCacheService;
            _logger = logger;
            
            // Путь к статическому CSS файлу
            _cssFilePath = Path.Combine(_env.WebRootPath, "css", "backgrounds.css");
            
            // Создаем директорию если её нет
            var cssDir = Path.GetDirectoryName(_cssFilePath);
            if (!Directory.Exists(cssDir))
            {
                Directory.CreateDirectory(cssDir!);
            }
        }

        public async Task<string> GenerateBackgroundCssFileAsync()
        {
            lock (_fileLock)
            {
                try
                {
                    var cssBuilder = new StringBuilder();
                    
                    // Заголовок файла
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine("/* BACKGROUNDS.CSS - STATIC BACKGROUND STYLES  */");
                    cssBuilder.AppendLine("/* Generated: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC */");
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine();
                    
                    // Глобальные стили для фонов
                    cssBuilder.AppendLine("/* === GLOBAL BACKGROUND CLASSES === */");
                    cssBuilder.AppendLine(".lazy-bg {");
                    cssBuilder.AppendLine("  position: relative;");
                    cssBuilder.AppendLine("  overflow: hidden;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    cssBuilder.AppendLine(".with-overlay {");
                    cssBuilder.AppendLine("  position: relative;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    cssBuilder.AppendLine(".background-overlay {");
                    cssBuilder.AppendLine("  content: '';");
                    cssBuilder.AppendLine("  position: absolute;");
                    cssBuilder.AppendLine("  top: 0;");
                    cssBuilder.AppendLine("  left: 0;");
                    cssBuilder.AppendLine("  width: 100%;");
                    cssBuilder.AppendLine("  height: 100%;");
                    cssBuilder.AppendLine("  z-index: 1;");
                    cssBuilder.AppendLine("  pointer-events: none;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    // Стили для видео фонов
                    cssBuilder.AppendLine("/* === VIDEO BACKGROUND CLASSES === */");
                    cssBuilder.AppendLine(".lazy-video {");
                    cssBuilder.AppendLine("  position: relative;");
                    cssBuilder.AppendLine("  min-height: 400px;");
                    cssBuilder.AppendLine("  overflow: hidden;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    cssBuilder.AppendLine(".lazy-video .video-container {");
                    cssBuilder.AppendLine("  position: absolute;");
                    cssBuilder.AppendLine("  top: 0;");
                    cssBuilder.AppendLine("  left: 0;");
                    cssBuilder.AppendLine("  width: 100%;");
                    cssBuilder.AppendLine("  height: 100%;");
                    cssBuilder.AppendLine("  z-index: 0;");
                    cssBuilder.AppendLine("  pointer-events: none;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    cssBuilder.AppendLine(".lazy-video .video-bg-iframe {");
                    cssBuilder.AppendLine("  position: absolute;");
                    cssBuilder.AppendLine("  top: 50%;");
                    cssBuilder.AppendLine("  left: 50%;");
                    cssBuilder.AppendLine("  width: 177.77777778vh;");
                    cssBuilder.AppendLine("  min-width: 100%;");
                    cssBuilder.AppendLine("  min-height: 100%;");
                    cssBuilder.AppendLine("  height: 56.25vw;");
                    cssBuilder.AppendLine("  transform: translate(-50%, -50%);");
                    cssBuilder.AppendLine("  border: 0;");
                    cssBuilder.AppendLine("  z-index: 0;");
                    cssBuilder.AppendLine("  pointer-events: none;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    cssBuilder.AppendLine(".lazy-video .video-placeholder {");
                    cssBuilder.AppendLine("  background-size: cover;");
                    cssBuilder.AppendLine("  background-position: center;");
                    cssBuilder.AppendLine("  position: absolute;");
                    cssBuilder.AppendLine("  top: 0;");
                    cssBuilder.AppendLine("  left: 0;");
                    cssBuilder.AppendLine("  width: 100%;");
                    cssBuilder.AppendLine("  height: 100%;");
                    cssBuilder.AppendLine("  z-index: -1;");
                    cssBuilder.AppendLine("}");
                    cssBuilder.AppendLine();
                    
                    // Секция для медиа классов
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine("/* MEDIA BACKGROUND CLASSES                   */");
                    cssBuilder.AppendLine("/* Auto-generated from Umbraco media library  */");
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine();
                    
                    // Секция для цветовых классов
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine("/* COLOR BACKGROUND CLASSES                   */");
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine();
                    
                    // Секция для градиентных классов
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine("/* GRADIENT BACKGROUND CLASSES                */");
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine();
                    
                    // Секция для оверлейных классов
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine("/* OVERLAY STYLES                             */");
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine();
                    
                    // Секция для пользовательских стилей (min-height и другие инлайновые)
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine("/* CUSTOM INLINE STYLES                       */");
                    cssBuilder.AppendLine("/* =========================================== */");
                    cssBuilder.AppendLine();
                    
                    var cssContent = cssBuilder.ToString();
                    
                    // Используем полное имя System.IO.File для устранения неоднозначности
                    System.IO.File.WriteAllText(_cssFilePath, cssContent, Encoding.UTF8);
                    
                    _logger.LogInformation($"Static CSS file generated: {_cssFilePath}");
                    return _cssFilePath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating static CSS file");
                    throw;
                }
            }
        }

        public async Task UpdateCssForMediaAsync(Guid mediaKey)
        {
            try
            {
                var media = _mediaService.GetById(mediaKey);
                if (media == null) return;
                
                var url = await _mediaCacheService.GetCachedMediaUrlAsync(mediaKey);
                if (string.IsNullOrEmpty(url)) return;
                
                var mediaHash = GetMediaHash(mediaKey, url);
                
                // Генерируем CSS для этого медиа
                var css = GenerateMediaCss(mediaKey, url);
                
                // Добавляем в файл
                await AppendOrUpdateCssSectionAsync($"media_{mediaKey:N}", css, "MEDIA BACKGROUND CLASSES");
                
                _logger.LogInformation($"Updated CSS for media: {mediaKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating CSS for media {mediaKey}");
            }
        }

        public async Task RemoveCssForMediaAsync(Guid mediaKey)
        {
            try
            {
                await RemoveCssSectionAsync($"media_{mediaKey:N}");
                _logger.LogInformation($"Removed CSS for media: {mediaKey}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing CSS for media {mediaKey}");
            }
        }

        public async Task RegenerateAllCssAsync()
        {
            try
            {
                // Сначала генерируем базовый файл
                await GenerateBackgroundCssFileAsync();
                
                // Получаем все медиа элементы - исправленная версия
                var allMedia = GetAllMediaItems();
                
                foreach (var media in allMedia)
                {
                    await UpdateCssForMediaAsync(media.Key);
                }
                
                _logger.LogInformation("Regenerated all CSS backgrounds");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating all CSS");
            }
        }

        public async Task<string> AddInlineStyleAsync(string css, string styleType = "custom")
        {
            var hash = ComputeHash(css);
            var className = $"{styleType}-{hash}";
            
            // Проверяем, есть ли уже такой стиль
            if (!_styleCache.ContainsKey(className))
            {
                _styleCache[className] = css;
                
                // Определяем секцию в зависимости от типа стиля
                string sectionName;
                switch (styleType.ToLower())
                {
                    case "video":
                        sectionName = "VIDEO BACKGROUND CLASSES";
                        break;
                    case "overlay":
                        sectionName = "OVERLAY STYLES";
                        break;
                    case "minheight":
                        sectionName = "CUSTOM INLINE STYLES";
                        // Для min-height генерируем класс
                        var minHeightMatch = Regex.Match(css, @"min-height:\s*(\d+)px");
                        if (minHeightMatch.Success)
                        {
                            var height = minHeightMatch.Groups[1].Value;
                            className = $"min-h-{height}";
                            css = $".{className} {{ {css} }}";
                        }
                        break;
                    default:
                        sectionName = "CUSTOM INLINE STYLES";
                        // Оборачиваем CSS в класс, если это не уже обернуто
                        if (!css.Trim().StartsWith("."))
                        {
                            css = $".{className} {{ {css} }}";
                        }
                        break;
                }
                
                await AppendOrUpdateCssSectionAsync(className, css, sectionName);
            }
            
            return className;
        }

        public async Task<string> GetOrAddMediaClassAsync(Guid mediaKey, string className, int minHeight = 400, string size = "cover", string position = "center")
        {
            var url = await _mediaCacheService.GetCachedMediaUrlAsync(mediaKey);
            if (string.IsNullOrEmpty(url)) return string.Empty;
            
            // Генерируем CSS
            var css = $@"
.{className} {{
    background-image: url('{url}');
    background-size: {size};
    background-position: {position};
    background-repeat: no-repeat;
    min-height: {minHeight}px;
}}";
            
            // Добавляем в файл
            var sectionId = $"media_{mediaKey:N}_{GetHash(className)}";
            await AppendOrUpdateCssSectionAsync(sectionId, css, "MEDIA BACKGROUND CLASSES");
            
            return className;
        }

        public async Task<string> GetOrAddColorClassAsync(string colorValue, int minHeight = 400)
        {
            var hash = ComputeHash($"color:{colorValue}:{minHeight}");
            var className = $"bg-color-{hash}";
            
            var css = $@"
.{className} {{
    background-color: {colorValue};
    min-height: {minHeight}px;
    position: relative;
}}";
            
            await AppendOrUpdateCssSectionAsync(className, css, "COLOR BACKGROUND CLASSES");
            
            return className;
        }

        public async Task<string> GetOrAddGradientClassAsync(string colorStart, string colorEnd, string direction = "to bottom", int minHeight = 400)
        {
            var hash = ComputeHash($"gradient:{colorStart}:{colorEnd}:{direction}:{minHeight}");
            var className = $"bg-gradient-{hash}";
            
            var css = $@"
.{className} {{
    background: linear-gradient({direction}, {colorStart}, {colorEnd});
    min-height: {minHeight}px;
    position: relative;
}}";
            
            await AppendOrUpdateCssSectionAsync(className, css, "GRADIENT BACKGROUND CLASSES");
            
            return className;
        }

        public async Task<string> AddOverlayStyleAsync(string overlayClass, string css)
        {
            // Добавляем префикс, чтобы стили оверлея были уникальными
            var sectionId = $"overlay_{overlayClass}";
            
            // Оборачиваем CSS в класс, если это не уже обернуто
            if (!css.Trim().StartsWith("."))
            {
                // Ищем уже существующие классы оверлея и добавляем к ним
                var overlayClassMatch = Regex.Match(css, @"\.([\w\-]+)\s*\.background-overlay");
                if (overlayClassMatch.Success)
                {
                    // CSS уже содержит правильный класс
                }
                else
                {
                    // Добавляем класс оверлея
                    css = $".{overlayClass} .background-overlay {{ {css} }}";
                }
            }
            
            await AppendOrUpdateCssSectionAsync(sectionId, css, "OVERLAY STYLES");
            return overlayClass;
        }

        private async Task AppendOrUpdateCssSectionAsync(string sectionId, string css, string sectionName)
        {
            lock (_fileLock)
            {
                try
                {
                    if (!System.IO.File.Exists(_cssFilePath))
                    {
                        GenerateBackgroundCssFileAsync().Wait();
                    }
                    
                    var content = System.IO.File.ReadAllText(_cssFilePath);
                    
                    // Ищем секцию по имени
                    var escapedSectionName = Regex.Escape(sectionName);
                    var sectionPattern = $@"\/\*\s*={{{escapedSectionName}}}\s*\*\/.*?(?=\/\*\s*=|\Z)";
                    var sectionMatch = Regex.Match(content, sectionPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    
                    if (sectionMatch.Success)
                    {
                        var sectionContent = sectionMatch.Value;
                        
                        // Ищем подсекцию с нашим ID
                        var subsectionPattern = $@"\/\*\s*{Regex.Escape(sectionId)}\s*\*\/.*?(?=\/\*\s*\w+|$)";
                        var subsectionMatch = Regex.Match(sectionContent, subsectionPattern, RegexOptions.Singleline);
                        
                        if (subsectionMatch.Success)
                        {
                            // Обновляем существующую подсекцию
                            var updatedSectionContent = Regex.Replace(
                                sectionContent,
                                Regex.Escape(subsectionMatch.Value),
                                $"/* {sectionId} */\n{css}\n",
                                RegexOptions.Singleline);
                            
                            content = content.Replace(sectionMatch.Value, updatedSectionContent);
                        }
                        else
                        {
                            // Добавляем новую подсекцию в конец секции
                            var updatedSectionContent = sectionContent.TrimEnd() + $"\n\n/* {sectionId} */\n{css}\n";
                            content = content.Replace(sectionMatch.Value, updatedSectionContent);
                        }
                    }
                    else
                    {
                        // Секция не найдена, добавляем новую
                        var newSection = $"\n\n/* =========================================== */\n" +
                                       $"/* {sectionName.PadRight(40)} */\n" +
                                       $"/* =========================================== */\n" +
                                       $"\n/* {sectionId} */\n{css}\n";
                        
                        content += newSection;
                    }
                    
                    System.IO.File.WriteAllText(_cssFilePath, content, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error appending CSS section: {sectionId}");
                    throw;
                }
            }
        }

        private async Task RemoveCssSectionAsync(string sectionId)
        {
            lock (_fileLock)
            {
                try
                {
                    if (!System.IO.File.Exists(_cssFilePath)) return;
                    
                    var content = System.IO.File.ReadAllText(_cssFilePath);
                    
                    // Ищем и удаляем подсекцию с нашим ID
                    var pattern = $@"\/\*\s*{Regex.Escape(sectionId)}\s*\*\/.*?(?=\/\*\s*\w+|$)";
                    content = Regex.Replace(content, pattern, "", RegexOptions.Singleline);
                    
                    // Удаляем пустые строки
                    content = Regex.Replace(content, @"^\s*$\n", "", RegexOptions.Multiline);
                    
                    System.IO.File.WriteAllText(_cssFilePath, content, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error removing CSS section: {sectionId}");
                    throw;
                }
            }
        }

        private string GenerateMediaCss(Guid mediaKey, string url)
        {
            var baseClass = $"bg-media-{mediaKey:N}";
            
            return $@"/* {mediaKey} */
.{baseClass} {{
    background-image: url('{url}');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}}

.{baseClass}-contain {{
    background-image: url('{url}');
    background-size: contain;
    background-position: center;
    background-repeat: no-repeat;
}}";
        }

        private string GetMediaHash(Guid mediaKey, string url)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var input = $"{mediaKey}:{url}";
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 8);
        }

        private string ComputeHash(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 8);
        }

        private string GetHash(string input)
        {
            return ComputeHash(input);
        }

        private List<IMedia> GetAllMediaItems()
        {
            var allMedia = new List<IMedia>();
            
            try
            {
                // Получаем корневые медиа
                var rootMedia = _mediaService.GetRootMedia().ToList();
                
                foreach (var media in rootMedia)
                {
                    allMedia.Add(media);
                    // Рекурсивно получаем дочерние элементы
                    GetDescendants(media, ref allMedia);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all media items");
            }
            
            return allMedia;
        }

        private void GetDescendants(IMedia parent, ref List<IMedia> allMedia)
        {
            try
            {
                // Используем GetPagedChildren с большим размером страницы, чтобы получить всех детей
                long totalRecords;
                var children = _mediaService.GetPagedChildren(parent.Id, 0, int.MaxValue, out totalRecords).ToList();
                
                foreach (var child in children)
                {
                    allMedia.Add(child);
                    GetDescendants(child, ref allMedia);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting descendants for media {parent.Id}");
            }
        }
    }
}