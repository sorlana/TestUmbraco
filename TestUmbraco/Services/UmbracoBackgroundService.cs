using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

namespace TestUmbraco.Services
{
    public class UmbracoBackgroundService : IUmbracoBackgroundService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UmbracoBackgroundService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public BackgroundResult ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg")
        {
            var result = new BackgroundResult();
            
            AddToDebugInfo($"ProcessBackground START: settings={settings != null}, componentId={componentId}, prefix={prefix}");
            
            if (settings == null)
            {
                AddToDebugInfo("ProcessBackground: settings is NULL, returning empty result");
                return result;
            }

            // Проверяем, есть ли настройки и выбрано ли значение в bg (вкладка Main)
            if (settings.HasProperty("bg") && settings.HasValue("bg"))
            {
                var bgValue = settings.Value<string>("bg");
                AddToDebugInfo($"ProcessBackground: bg value = '{bgValue}'");
                
                switch (bgValue)
                {
                    case "Изображение":
                        AddToDebugInfo("ProcessBackground: processing Image background");
                        result = ProcessImageBackground(settings, componentId, prefix);
                        break;
                    case "Цвет":
                        AddToDebugInfo("ProcessBackground: processing Color background");
                        result = ProcessColorBackground(settings, componentId, prefix);
                        break;
                    case "Градиент":
                        AddToDebugInfo("ProcessBackground: processing Gradient background");
                        result = ProcessGradientBackground(settings, componentId, prefix);
                        break;
                    case "Видео":
                        AddToDebugInfo("ProcessBackground: processing Video background");
                        result = ProcessVideoBackground(settings, componentId, prefix);
                        break;
                    default:
                        AddToDebugInfo($"ProcessBackground: unknown bg value '{bgValue}'");
                        break;
                }
            }
            else
            {
                AddToDebugInfo($"ProcessBackground: no bg property or value. HasProperty={settings.HasProperty("bg")}, HasValue={settings.HasValue("bg")}");
            }
            
            AddToDebugInfo($"ProcessBackground END: Type={result.Type}, HasBackground={result.HasBackground}, CssClass='{result.CssClass}'");
            
            return result;
        }

        private BackgroundResult ProcessImageBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Image };
            AddToDebugInfo($"ProcessImageBackground START for component {componentId}");
            
            // Проверяем наличие фонового изображения (свойство backgroundImage из вкладки BackgroundColor)
            if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
            {
                AddToDebugInfo("ProcessImageBackground: backgroundImage property exists and has value");
                
                var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                if (bgImage != null)
                {
                    AddToDebugInfo($"ProcessImageBackground: bgImage found, Key={bgImage.Key}");
                    
                    // Создаем уникальный класс для компонента
                    var bgClass = $"{prefix}-img-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                    
                    // Получаем минимальную высоту (если есть)
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    // Получаем значение bgSize и преобразуем в CSS-значение (свойство bgSize из вкладки BackgroundColor)
                    var bgSize = "auto"; // значение по умолчанию, если ничего не выбрано
                    if (settings.HasProperty("bgSize") && settings.HasValue("bgSize"))
                    {
                        var bgSizeValue = settings.Value<string>("bgSize");
                        if (!string.IsNullOrWhiteSpace(bgSizeValue))
                        {
                            bgSize = ConvertBgSizeToCss(bgSizeValue);
                        }
                    }
                    
                    // Позиция фона (если есть)
                    var bgPosition = settings.HasValue("backgroundPosition") 
                        ? settings.Value<string>("backgroundPosition") 
                        : "center";
                    
                    AddToDebugInfo($"ProcessImageBackground: settings - minHeight={minHeight}, bgSize={bgSize}, bgPosition={bgPosition}");
                    
                    // Формируем параметр для API
                    var backgroundParam = $"{bgImage.Key}:{bgClass}:{minHeight}:{bgSize}:{bgPosition}";
                    
                    // Добавляем в Context.Items
                    AddToBackgroundParams(backgroundParam);
                    
                    result.CssClass = bgClass;
                    result.HasBackground = true;
                    
                    AddToDebugInfo($"ProcessImageBackground: created class '{bgClass}', added to BackgroundParams");
                }
                else
                {
                    AddToDebugInfo("ProcessImageBackground: bgImage is NULL after Value<IPublishedContent>");
                }
            }
            else
            {
                AddToDebugInfo($"ProcessImageBackground: no backgroundImage property. HasProperty={settings.HasProperty("backgroundImage")}, HasValue={settings.HasValue("backgroundImage")}");
            }
            
            AddToDebugInfo($"ProcessImageBackground END for component {componentId}");
            return result;
        }

        private BackgroundResult ProcessColorBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Color };
            AddToDebugInfo($"ProcessColorBackground START for component {componentId}");
            
            // Используем свойство color из вкладки BackgroundColor
            string color = null;
            
            if (settings.HasProperty("color") && settings.HasValue("color"))
            {
                color = settings.Value<string>("color");
                AddToDebugInfo($"ProcessColorBackground: found color '{color}' in property 'color'");
            }
            else
            {
                AddToDebugInfo("ProcessColorBackground: no color property found");
            }
            
            if (!string.IsNullOrWhiteSpace(color))
            {
                var bgClass = $"{prefix}-color-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                
                // Получаем минимальную высоту
                var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                
                // Генерируем CSS для цвета
                var css = $".{bgClass} {{ background-color: {color} !important; min-height: {minHeight}px !important; }}";
                
                result.CssClass = bgClass;
                result.CssContent = css;
                result.HasBackground = true;
                
                // Добавляем CSS в коллекцию
                AddToCssStyles(css);
                
                AddToDebugInfo($"ProcessColorBackground: created class '{bgClass}', CSS: {css}");
            }
            else
            {
                AddToDebugInfo("ProcessColorBackground: color value is empty or whitespace");
            }
            
            AddToDebugInfo($"ProcessColorBackground END for component {componentId}");
            return result;
        }

        private BackgroundResult ProcessGradientBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Gradient };
            AddToDebugInfo($"ProcessGradientBackground START for component {componentId}");
            
            // Используем свойства colorStart, colorEnd и direction из вкладки BackgroundColor
            string colorStart = null;
            string colorEnd = null;
            
            if (settings.HasProperty("colorStart") && settings.HasValue("colorStart") &&
                settings.HasProperty("colorEnd") && settings.HasValue("colorEnd"))
            {
                colorStart = settings.Value<string>("colorStart");
                colorEnd = settings.Value<string>("colorEnd");
                AddToDebugInfo($"ProcessGradientBackground: using colorStart/colorEnd: {colorStart} -> {colorEnd}");
            }
            else
            {
                AddToDebugInfo($"ProcessGradientBackground: missing required color properties. Tried: colorStart/colorEnd");
            }
            
            if (!string.IsNullOrWhiteSpace(colorStart) && !string.IsNullOrWhiteSpace(colorEnd))
            {
                var bgClass = $"{prefix}-gradient-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                
                // Получаем направление градиента (свойство direction из вкладки BackgroundColor)
                var direction = "to bottom"; // по умолчанию сверху вниз
                
                if (settings.HasProperty("direction") && settings.HasValue("direction"))
                {
                    var directionValue = settings.Value<string>("direction");
                    AddToDebugInfo($"ProcessGradientBackground: direction value = '{directionValue}'");
                    direction = ConvertDirectionToCss(directionValue);
                }
                else
                {
                    AddToDebugInfo("ProcessGradientBackground: using default direction 'to bottom'");
                }
                
                // Получаем минимальную высоту
                var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                
                // Генерируем CSS для градиента
                var css = $".{bgClass} {{ " +
                          $"background: linear-gradient({direction}, {colorStart}, {colorEnd}) !important; " +
                          $"min-height: {minHeight}px !important; " +
                          $"}}";
                
                result.CssClass = bgClass;
                result.CssContent = css;
                result.HasBackground = true;
                
                // Добавляем CSS в коллекцию
                AddToCssStyles(css);
                
                AddToDebugInfo($"ProcessGradientBackground: created class '{bgClass}', CSS: {css}");
            }
            else
            {
                AddToDebugInfo("ProcessGradientBackground: missing required color properties");
            }
            
            AddToDebugInfo($"ProcessGradientBackground END for component {componentId}");
            return result;
        }

        private BackgroundResult ProcessVideoBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Video };
            AddToDebugInfo($"ProcessVideoBackground START for component {componentId}");
            
            // Используем свойство video из вкладки BackgroundColor
            string videoUrl = null;
            
            if (settings.HasProperty("video") && settings.HasValue("video"))
            {
                videoUrl = settings.Value<string>("video");
                AddToDebugInfo($"ProcessVideoBackground: found video URL in property 'video': '{videoUrl}'");
            }
            else
            {
                AddToDebugInfo("ProcessVideoBackground: no video property found");
                
                // Дополнительная отладка: покажем все доступные свойства
                AddToDebugInfo("ProcessVideoBackground: Available properties:");
                foreach (var prop in settings.Properties)
                {
                    AddToDebugInfo($"  - {prop.Alias}: HasValue={settings.HasValue(prop.Alias)}");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(videoUrl))
            {
                // Извлекаем ID видео из ссылки Vimeo
                var videoId = ExtractVimeoVideoId(videoUrl);
                AddToDebugInfo($"ProcessVideoBackground: extracted videoId = '{videoId}'");
                
                if (!string.IsNullOrEmpty(videoId))
                {
                    // Создаем уникальный класс для компонента
                    var bgClass = $"{prefix}-video-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                    
                    // Получаем минимальную высоту
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    // Получаем размер видео (используем то же свойство bgSize)
                    var bgSize = "cover"; // по умолчанию
                    if (settings.HasProperty("bgSize") && settings.HasValue("bgSize"))
                    {
                        var bgSizeValue = settings.Value<string>("bgSize");
                        if (!string.IsNullOrWhiteSpace(bgSizeValue))
                        {
                            bgSize = ConvertBgSizeToCss(bgSizeValue);
                        }
                    }
                    
                    // Получаем позицию видео
                    var bgPosition = settings.HasValue("backgroundPosition") 
                        ? settings.Value<string>("backgroundPosition") 
                        : "center";
                    
                    AddToDebugInfo($"ProcessVideoBackground: minHeight={minHeight}, bgSize={bgSize}, bgPosition={bgPosition}");
                    
                    // Генерируем HTML для встраивания видео Vimeo
                    var html = GenerateVimeoEmbedHtml(videoId, bgClass, bgSize, bgPosition);
                    
                    // CSS для позиционирования
                    var css = $@"
.{bgClass} {{
    position: relative !important;
    overflow: hidden !important;
    min-height: {minHeight}px !important;
}}";
                    
                    result.CssClass = bgClass;
                    result.HtmlContent = html;
                    result.CssContent = css;
                    result.HasBackground = true;
                    
                    // Добавляем CSS в общую коллекцию
                    AddToCssStyles(css);
                    
                    AddToDebugInfo($"ProcessVideoBackground: created class '{bgClass}', HTML length={html.Length}");
                }
                else
                {
                    AddToDebugInfo("ProcessVideoBackground: could not extract videoId from URL");
                }
            }
            else
            {
                AddToDebugInfo("ProcessVideoBackground: videoUrl value is empty or not found");
            }
            
            AddToDebugInfo($"ProcessVideoBackground END for component {componentId}");
            return result;
        }

        private string ExtractVimeoVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                AddToDebugInfo($"ExtractVimeoVideoId: URL is null or empty");
                return null;
            }
            
            AddToDebugInfo($"ExtractVimeoVideoId: processing URL '{url}'");
            
            // Убираем параметры запроса если есть
            url = url.Split('?')[0];
            
            // Пробуем разные форматы ссылок Vimeo
            var patterns = new[]
            {
                @"vimeo\.com/(?:.*/)?(\d+)",
                @"player\.vimeo\.com/video/(\d+)",
                @"vimeo\.com/channels/[^/]+/(\d+)",
                @"vimeo\.com/groups/[^/]+/videos/(\d+)"
            };
            
            foreach (var pattern in patterns)
            {
                try
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    var match = regex.Match(url);
                    
                    if (match.Success && match.Groups.Count > 1)
                    {
                        var id = match.Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(id) && id.All(char.IsDigit))
                        {
                            AddToDebugInfo($"ExtractVimeoVideoId: found ID '{id}' with pattern '{pattern}'");
                            return id;
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddToDebugInfo($"ExtractVimeoVideoId: error with pattern '{pattern}': {ex.Message}");
                }
            }
            
            AddToDebugInfo($"ExtractVimeoVideoId: no video ID found in URL");
            return null;
        }

        private string GenerateVimeoEmbedHtml(string videoId, string bgClass, string bgSize, string bgPosition = "center")
        {
            AddToDebugInfo($"GenerateVimeoEmbedHtml: videoId='{videoId}', bgClass='{bgClass}', bgSize='{bgSize}', bgPosition='{bgPosition}'");
            
            // Преобразуем позицию в CSS
            var positionCss = bgPosition switch
            {
                "top" => "top center",
                "bottom" => "bottom center",
                "left" => "center left",
                "right" => "center right",
                "top left" => "top left",
                "top right" => "top right",
                "bottom left" => "bottom left",
                "bottom right" => "bottom right",
                _ => "center"
            };
            
            // Параметры для Vimeo фонового видео:
            // - autoplay=1: автоматическое воспроизведение
            // - background=1: режим фонового видео (скрывает элементы управления)
            // - muted=1: отключение звука (обязательно для автовоспроизведения)
            // - loop=1: зацикливание видео
            // - autopause=0: не останавливать видео при переключении вкладок
            
            var embedUrl = $"https://player.vimeo.com/video/{videoId}?autoplay=1&background=1&muted=1&loop=1&autopause=0";
            
            var html = $@"
<div class='{bgClass}-container' style='
    position: absolute !important;
    top: 0 !important;
    left: 0 !important;
    width: 100% !important;
    height: 100% !important;
    overflow: hidden !important;
    z-index: 0 !important;
'>
    <iframe 
        src='{embedUrl}'
        style='
            position: absolute !important;
            top: 50% !important;
            left: 50% !important;
            transform: translate(-50%, -50%) !important;
            width: 100vw !important;
            height: 100vh !important;
            object-fit: {bgSize} !important;
            object-position: {positionCss} !important;
        '
        frameborder='0'
        allow='autoplay; fullscreen'
        allowfullscreen
        title='Vimeo background video'>
    </iframe>
</div>";
            
            AddToDebugInfo($"GenerateVimeoEmbedHtml: generated HTML length = {html.Length}");
            return html;
        }

        private string ConvertBgSizeToCss(string bgSizeValue)
        {
            if (string.IsNullOrWhiteSpace(bgSizeValue))
            {
                AddToDebugInfo($"ConvertBgSizeToCss: empty value, returning 'auto'");
                return "auto";
            }
            
            var trimmedValue = bgSizeValue.Trim();
            AddToDebugInfo($"ConvertBgSizeToCss: converting '{trimmedValue}'");
            
            var result = trimmedValue switch
            {
                "Как есть" => "auto",
                "По ширине" => "100% auto",
                "По высоте" => "auto 100%",
                "Обложка" => "cover",
                "Вместить" => "contain",
                _ => "auto"
            };
            
            AddToDebugInfo($"ConvertBgSizeToCss: result = '{result}'");
            return result;
        }

        private string ConvertDirectionToCss(string directionValue)
        {
            if (string.IsNullOrWhiteSpace(directionValue))
            {
                AddToDebugInfo($"ConvertDirectionToCss: empty value, returning 'to bottom'");
                return "to bottom";
            }
            
            var trimmedValue = directionValue.Trim();
            AddToDebugInfo($"ConvertDirectionToCss: converting '{trimmedValue}'");
            
            var result = trimmedValue switch
            {
                "Сверху вниз" => "to bottom",
                "Снизу вверх" => "to top",
                "Слева направо" => "to right",
                "Справа налево" => "to left",
                _ => "to bottom" // по умолчанию
            };
            
            AddToDebugInfo($"ConvertDirectionToCss: result = '{result}'");
            return result;
        }

        private void AddToBackgroundParams(string backgroundParam)
        {
            AddToDebugInfo($"AddToBackgroundParams: adding param '{backgroundParam}'");
            
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                AddToDebugInfo("AddToBackgroundParams: HttpContext is NULL");
                return;
            }

            var backgroundParams = httpContext.Items["BackgroundParams"] as List<string>;
            if (backgroundParams == null)
            {
                backgroundParams = new List<string>();
                httpContext.Items["BackgroundParams"] = backgroundParams;
                AddToDebugInfo("AddToBackgroundParams: created new BackgroundParams list");
            }
            
            backgroundParams.Add(backgroundParam);
            AddToDebugInfo($"AddToBackgroundParams: added. Now {backgroundParams.Count} items in list");
        }

        private void AddToCssStyles(string css)
        {
            AddToDebugInfo($"AddToCssStyles: adding CSS (length={css.Length})");
            
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                AddToDebugInfo("AddToCssStyles: HttpContext is NULL");
                return;
            }

            var cssStyles = httpContext.Items["CssStyles"] as List<string>;
            if (cssStyles == null)
            {
                cssStyles = new List<string>();
                httpContext.Items["CssStyles"] = cssStyles;
                AddToDebugInfo("AddToCssStyles: created new CssStyles list");
            }
            
            cssStyles.Add(css);
            AddToDebugInfo($"AddToCssStyles: added. Now {cssStyles.Count} items in list");
        }

        private void AddToDebugInfo(string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var debugMessages = httpContext.Items["BackgroundDebug"] as List<string>;
            if (debugMessages == null)
            {
                debugMessages = new List<string>();
                httpContext.Items["BackgroundDebug"] = debugMessages;
            }
            debugMessages.Add($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
    }
}