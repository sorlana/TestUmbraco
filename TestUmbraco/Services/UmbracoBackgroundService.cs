using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Models.PublishedContent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using TestUmbraco.Services;

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
            
            if (settings == null)
                return result;

            if (settings.HasProperty("bg") && settings.HasValue("bg"))
            {
                var bgValue = settings.Value<string>("bg");
                
                // Обрабатываем основной фон
                result = ProcessBackgroundType(settings, componentId, prefix, bgValue);
                
                // Обрабатываем оверлей (ТОЛЬКО если выбрано значение)
                if (settings.HasProperty("overlayBg") && settings.HasValue("overlayBg"))
                {
                    var overlayBgValue = settings.Value<string>("overlayBg");
                    
                    if (!string.IsNullOrWhiteSpace(overlayBgValue) && overlayBgValue != "Не выбрано" && overlayBgValue != "None")
                    {
                        result = ProcessOverlay(settings, componentId, prefix, overlayBgValue, result);
                    }
                }
            }
            
            return result;
        }

        private BackgroundResult ProcessBackgroundType(IPublishedElement settings, Guid componentId, string prefix, string? bgValue)
        {
            var result = new BackgroundResult();
            
            if (string.IsNullOrWhiteSpace(bgValue))
                return result;
            
            var trimmedValue = bgValue.Trim();
            
            switch (trimmedValue)
            {
                case "Изображение":
                    result = ProcessImageBackground(settings, componentId, prefix);
                    break;
                case "Цвет":
                    result = ProcessColorBackground(settings, componentId, prefix);
                    break;
                case "Градиент":
                    result = ProcessGradientBackground(settings, componentId, prefix);
                    break;
                case "Видео":
                    result = ProcessVideoBackground(settings, componentId, prefix);
                    break;
            }
            
            return result;
        }

        private BackgroundResult ProcessOverlay(IPublishedElement settings, Guid componentId, string prefix, string overlayBgValue, BackgroundResult existingResult)
        {
            if (!existingResult.HasBackground)
                return existingResult;
            
            // Создаем CSS для оверлея
            var overlayCss = GenerateOverlayCss(settings, overlayBgValue, existingResult.CssClass, existingResult.Type);
            
            if (!string.IsNullOrEmpty(overlayCss))
            {
                AddToCssStyles(overlayCss);
            }
            
            // Устанавливаем флаг наличия оверлея
            existingResult.HasOverlay = !string.IsNullOrEmpty(overlayBgValue) && 
                                       overlayBgValue != "Не выбрано" && 
                                       overlayBgValue != "None";
            
            return existingResult;
        }

        private BackgroundResult ProcessImageBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Image };
            
            if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
            {
                var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                if (bgImage != null)
                {
                    var bgClass = $"{prefix}-img-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                    
                    // Получаем параметры
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    var bgSize = "auto";
                    var bgPosition = "center";
                    
                    if (settings.HasProperty("bgSize") && settings.HasValue("bgSize"))
                    {
                        var bgSizeValue = settings.Value<string>("bgSize");
                        if (!string.IsNullOrWhiteSpace(bgSizeValue))
                        {
                            bgSize = ConvertBgSizeToCss(bgSizeValue);
                        }
                    }
                    
                    bgPosition = settings.HasValue("backgroundPosition") 
                        ? settings.Value<string>("backgroundPosition") ?? "center"
                        : "center";
                    
                    // Формируем параметр для API
                    var backgroundParam = $"{bgImage.Key}:{bgClass}:{minHeight}:{bgSize}:{bgPosition}";
                    
                    AddToBackgroundParams(backgroundParam);
                    
                    // CSS для позиционирования
                    var css = $@"
.{bgClass} {{
    position: relative;
    min-height: {minHeight}px;
}}";
                    
                    AddToCssStyles(css);
                    
                    result.CssClass = bgClass;
                    result.HasBackground = true;
                }
            }
            
            return result;
        }

        private BackgroundResult ProcessColorBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Color };
            
            if (settings.HasProperty("color") && settings.HasValue("color"))
            {
                var color = settings.Value<string>("color");
                if (!string.IsNullOrWhiteSpace(color))
                {
                    var bgClass = $"{prefix}-color-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    var css = $@"
.{bgClass} {{
    background-color: {color};
    min-height: {minHeight}px;
    position: relative;
}}";
                    
                    result.CssClass = bgClass;
                    result.HasBackground = true;
                    AddToCssStyles(css);
                }
            }
            
            return result;
        }

        private BackgroundResult ProcessGradientBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Gradient };
            
            if (settings.HasProperty("colorStart") && settings.HasValue("colorStart") &&
                settings.HasProperty("colorEnd") && settings.HasValue("colorEnd"))
            {
                var colorStart = settings.Value<string>("colorStart");
                var colorEnd = settings.Value<string>("colorEnd");
                
                if (!string.IsNullOrWhiteSpace(colorStart) && !string.IsNullOrWhiteSpace(colorEnd))
                {
                    var bgClass = $"{prefix}-gradient-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                    
                    var direction = "to bottom";
                    if (settings.HasProperty("direction") && settings.HasValue("direction"))
                    {
                        var directionValue = settings.Value<string>("direction");
                        direction = ConvertDirectionToCss(directionValue);
                    }
                    
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    var css = $@"
.{bgClass} {{
    background: linear-gradient({direction}, {colorStart}, {colorEnd});
    min-height: {minHeight}px;
    position: relative;
}}";
                    
                    result.CssClass = bgClass;
                    result.HasBackground = true;
                    AddToCssStyles(css);
                }
            }
            
            return result;
        }

        private BackgroundResult ProcessVideoBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Video };
            
            if (settings.HasProperty("video") && settings.HasValue("video"))
            {
                var videoUrl = settings.Value<string>("video");
                if (!string.IsNullOrWhiteSpace(videoUrl))
                {
                    var videoId = ExtractVimeoVideoId(videoUrl);
                    if (!string.IsNullOrEmpty(videoId))
                    {
                        var bgClass = $"{prefix}-video-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                        var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                        
                        // Получаем размер видео
                        var bgSize = "cover";
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
                            ? settings.Value<string>("backgroundPosition") ?? "center"
                            : "center";
                        
                        // Генерируем CSS
                        var css = GenerateVideoCss(videoId, bgClass, minHeight, bgSize, bgPosition);
                        
                        // Генерируем HTML
                        var html = GenerateVideoHtml(videoId, bgSize, bgPosition);
                        
                        result.CssClass = bgClass;
                        result.HasBackground = true;
                        result.HtmlContent = html;
                        AddToCssStyles(css);
                    }
                }
            }
            
            return result;
        }

        private string GenerateVideoHtml(string videoId, string bgSize, string bgPosition)
        {
            // Преобразуем настройки в CSS свойства
            var objectFit = ConvertBgSizeToObjectFit(bgSize);
            var objectPosition = ConvertBgPositionToObjectPosition(bgPosition);
            
            var html = $@"
<iframe class='video-bg' 
        src='https://player.vimeo.com/video/{videoId}?autoplay=1&background=1&muted=1&loop=1&autopause=0'
        allow='autoplay; fullscreen'
        allowfullscreen
        title='Vimeo background video'
        style='object-fit: {objectFit}; object-position: {objectPosition};'>
</iframe>";
            
            return html;
        }

        private string GenerateOverlayCss(IPublishedElement settings, string overlayType, string mainClass, BackgroundType bgType)
        {
            var cssBuilder = new StringBuilder();
            
            // Для видео-фона оверлей должен быть выше видео
            if (bgType == BackgroundType.Video)
            {
                cssBuilder.Append($@"
.{mainClass}.with-overlay::before {{
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 1;
}}");
            }
            else
            {
                cssBuilder.Append($@"
.{mainClass}::before {{
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 0;
}}");
            }
            
            switch (overlayType)
            {
                case "Цвет":
                    if (settings.HasProperty("colorOverlay") && settings.HasValue("colorOverlay"))
                    {
                        var color = settings.Value<string>("colorOverlay");
                        if (!string.IsNullOrWhiteSpace(color))
                        {
                            if (bgType == BackgroundType.Video)
                            {
                                cssBuilder.Append($@"
.{mainClass}.with-overlay::before {{
    background-color: {color};
}}");
                            }
                            else
                            {
                                cssBuilder.Append($@"
.{mainClass}::before {{
    background-color: {color};
}}");
                            }
                        }
                    }
                    break;
                    
                case "Изображение":
                    if (settings.HasProperty("imageOverlay") && settings.HasValue("imageOverlay"))
                    {
                        var image = settings.Value<IPublishedContent>("imageOverlay");
                        if (image != null)
                        {
                            var bgSize = "cover";
                            var bgPosition = "center";
                            var repeat = "no-repeat";
                            
                            if (settings.HasProperty("bgSizeOverlay") && settings.HasValue("bgSizeOverlay"))
                            {
                                var bgSizeValue = settings.Value<string>("bgSizeOverlay");
                                if (!string.IsNullOrWhiteSpace(bgSizeValue))
                                {
                                    bgSize = ConvertBgSizeToCss(bgSizeValue);
                                }
                            }
                            
                            if (settings.HasProperty("repeatOverlay") && settings.HasValue("repeatOverlay"))
                            {
                                var repeatValue = settings.Value<bool>("repeatOverlay");
                                repeat = repeatValue ? "repeat" : "no-repeat";
                            }
                            
                            if (bgType == BackgroundType.Video)
                            {
                                cssBuilder.Append($@"
.{mainClass}.with-overlay::before {{
    background-image: url('/umbraco/api/media/get?key={image.Key}');
    background-size: {bgSize};
    background-position: {bgPosition};
    background-repeat: {repeat};
}}");
                            }
                            else
                            {
                                cssBuilder.Append($@"
.{mainClass}::before {{
    background-image: url('/umbraco/api/media/get?key={image.Key}');
    background-size: {bgSize};
    background-position: {bgPosition};
    background-repeat: {repeat};
}}");
                            }
                        }
                    }
                    break;
                    
                case "Градиент":
                    if (settings.HasProperty("colorStartOverlay") && settings.HasValue("colorStartOverlay") &&
                        settings.HasProperty("colorEndOverlay") && settings.HasValue("colorEndOverlay"))
                    {
                        var colorStart = settings.Value<string>("colorStartOverlay");
                        var colorEnd = settings.Value<string>("colorEndOverlay");
                        
                        var direction = "to bottom";
                        if (settings.HasProperty("directionOverlay") && settings.HasValue("directionOverlay"))
                        {
                            var directionValue = settings.Value<string>("directionOverlay");
                            direction = ConvertDirectionToCss(directionValue);
                        }
                        
                        if (bgType == BackgroundType.Video)
                        {
                            cssBuilder.Append($@"
.{mainClass}.with-overlay::before {{
    background: linear-gradient({direction}, {colorStart}, {colorEnd});
}}");
                        }
                        else
                        {
                            cssBuilder.Append($@"
.{mainClass}::before {{
    background: linear-gradient({direction}, {colorStart}, {colorEnd});
}}");
                        }
                    }
                    break;
            }
            
            // Прозрачность для оверлея
            if (settings.HasProperty("opacityOverlay") && settings.HasValue("opacityOverlay"))
            {
                var opacityValue = settings.Value<int>("opacityOverlay");
                var opacity = opacityValue / 100.0;
                
                if (bgType == BackgroundType.Video)
                {
                    cssBuilder.Append($@"
.{mainClass}.with-overlay::before {{
    opacity: {opacity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)};
}}");
                }
                else
                {
                    cssBuilder.Append($@"
.{mainClass}::before {{
    opacity: {opacity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)};
}}");
                }
            }
            
            // Базовые CSS для контейнера с контентом
            cssBuilder.Append($@"
.{mainClass} .container {{
    position: relative;
}}");

            if (bgType == BackgroundType.Video)
            {
                cssBuilder.Append($@"
.{mainClass}.with-overlay .container {{
    z-index: 2;
}}");
            }
            else
            {
                cssBuilder.Append($@"
.{mainClass} .container {{
    z-index: 1;
}}");
            }
            
            return cssBuilder.ToString();
        }

        private string GenerateVideoCss(string videoId, string bgClass, int minHeight, string bgSize, string bgPosition)
        {
            // Преобразуем bgSize в object-fit для видео
            var objectFit = ConvertBgSizeToObjectFit(bgSize);
            
            // Преобразуем bgPosition в object-position для видео
            var objectPosition = ConvertBgPositionToObjectPosition(bgPosition);
            
            var css = $@"
.{bgClass} {{
    position: relative;
    min-height: {minHeight}px;
    overflow: hidden;
}}
.{bgClass} .video-bg {{
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 0;
    pointer-events: none;
    border: 0;
}}";
            
            return css;
        }

        private string ConvertBgSizeToObjectFit(string bgSize)
        {
            return bgSize switch
            {
                "cover" => "cover",
                "contain" => "contain",
                "100% auto" => "cover", // По ширине - обрезать по высоте
                "auto 100%" => "cover", // По высоте - обрезать по ширине
                _ => "cover" // По умолчанию
            };
        }

        private string ConvertBgPositionToObjectPosition(string bgPosition)
        {
            return bgPosition switch
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
        }

        private string? ExtractVimeoVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            
            url = url.Split('?')[0];
            
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
                            return id;
                        }
                    }
                }
                catch { }
            }
            
            return null;
        }

        private string ConvertBgSizeToCss(string bgSizeValue)
        {
            if (string.IsNullOrWhiteSpace(bgSizeValue))
                return "auto";
            
            var trimmedValue = bgSizeValue.Trim();
            
            return trimmedValue switch
            {
                "Как есть" => "auto",
                "По ширине" => "100% auto",
                "По высоте" => "auto 100%",
                "Обложка" => "cover",
                "Вместить" => "contain",
                _ => "auto"
            };
        }

        private string ConvertDirectionToCss(string? directionValue)
        {
            if (string.IsNullOrWhiteSpace(directionValue))
                return "to bottom";
            
            var trimmedValue = directionValue.Trim();
            
            return trimmedValue switch
            {
                "Сверху вниз" => "to bottom",
                "Снизу вверх" => "to top",
                "Слева направо" => "to right",
                "Справа налево" => "to left",
                _ => "to bottom"
            };
        }

        private void AddToBackgroundParams(string backgroundParam)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var backgroundParams = httpContext.Items["BackgroundParams"] as List<string>;
            if (backgroundParams == null)
            {
                backgroundParams = new List<string>();
                httpContext.Items["BackgroundParams"] = backgroundParams;
            }
            
            backgroundParams.Add(backgroundParam);
        }

        private void AddToCssStyles(string css)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var cssStyles = httpContext.Items["BackgroundCss"] as List<string>;
            if (cssStyles == null)
            {
                cssStyles = new List<string>();
                httpContext.Items["BackgroundCss"] = cssStyles;
            }
            
            cssStyles.Add(css);
        }
    }
}