// Services/UmbracoBackgroundService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using System.Text;
using TestUmbraco.Services;
using System.Text.Json;

namespace TestUmbraco.Services
{
    public class UmbracoBackgroundService : IUmbracoBackgroundService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UmbracoBackgroundService> _logger;

        public UmbracoBackgroundService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<UmbracoBackgroundService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public BackgroundResult ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg")
        {
            var result = new BackgroundResult();
            
            if (settings == null) return result;

            if (settings.HasProperty("bg") && settings.HasValue("bg"))
            {
                var bgValue = settings.Value<string>("bg");
                if (!string.IsNullOrWhiteSpace(bgValue))
                {
                    result = ProcessBackgroundType(settings, componentId, prefix, bgValue);
                    
                    // Обработка оверлея
                    if (result.HasBackground && settings.HasProperty("overlayBg") && settings.HasValue("overlayBg"))
                    {
                        var overlayBgValue = settings.Value<string>("overlayBg");
                        if (!string.IsNullOrWhiteSpace(overlayBgValue) && overlayBgValue != "Не выбрано" && overlayBgValue != "None")
                        {
                            result.CssClass += " with-overlay";
                            // Добавляем специальный класс для оверлея
                            var overlayClass = $"overlay-{componentId.ToString("N").Substring(0, 8)}";
                            result.CssClass += $" {overlayClass}";
                            AddOverlayStyles(settings, result.CssClass, componentId, overlayClass);
                        }
                    }
                    
                    // Регистрируем информацию для JavaScript
                    RegisterBackgroundInfo(settings, componentId, result, bgValue);
                }
            }
            
            return result;
        }

        private BackgroundResult ProcessBackgroundType(IPublishedElement settings, Guid componentId, string prefix, string bgValue)
        {
            var result = new BackgroundResult();
            
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

        private BackgroundResult ProcessImageBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Image };
            
            if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
            {
                var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                if (bgImage != null)
                {
                    var bgClass = $"{prefix}-img-{componentId.ToString().Replace("-", "").Substring(0, 8)}";
                    var imageUrl = bgImage.Url();
                    
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    var bgSize = settings.HasValue("bgSize") ? 
                        ConvertBgSizeToCss(settings.Value<string>("bgSize") ?? "") : "cover";
                    var bgPosition = settings.HasValue("backgroundPosition") ? 
                        settings.Value<string>("backgroundPosition") ?? "center" : "center";
                    
                    // CSS для изображений
                    var css = $@"
.{bgClass}.lazy-image {{
    min-height: {minHeight}px;
    position: relative;
    background-color: #f5f5f5;
}}

.{bgClass}.lazy-image.bg-loaded {{
    background-image: url('{imageUrl}');
    background-size: {bgSize};
    background-position: {bgPosition};
    background-repeat: no-repeat;
    background-color: transparent;
}}";
                    
                    AddToCssStyles(css);
                    
                    result.CssClass = $"{bgClass} lazy-image";
                    result.HasBackground = true;
                    result.IsLazyLoaded = true;
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
                    
                    var css = $".{bgClass} {{ background-color: {color}; min-height: {minHeight}px; position: relative; }}";
                    AddToCssStyles(css);
                    
                    result.CssClass = bgClass;
                    result.HasBackground = true;
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
                        direction = ConvertDirectionToCss(settings.Value<string>("direction") ?? "");
                    }
                    
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    var css = $".{bgClass} {{ background: linear-gradient({direction}, {colorStart}, {colorEnd}); min-height: {minHeight}px; position: relative; }}";
                    AddToCssStyles(css);
                    
                    result.CssClass = bgClass;
                    result.HasBackground = true;
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
                        
                        // Стили для видео, которые точно работают с Vimeo
                        var css = $@"
.{bgClass}.lazy-video {{
    position: relative;
    min-height: {minHeight}px;
    overflow: hidden;
}}

.{bgClass}.lazy-video .video-container {{
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 0;
    pointer-events: none;
}}

.{bgClass}.lazy-video .video-bg-iframe {{
    position: absolute;
    top: 50%;
    left: 50%;
    width: 177.77777778vh;
    min-width: 100%;
    min-height: 100%;
    height: 56.25vw;
    transform: translate(-50%, -50%);
    border: 0;
    z-index: 0;
    pointer-events: none;
}}";
                        
                        AddToCssStyles(css);
                        
                        result.CssClass = $"{bgClass} lazy-video";
                        result.HasBackground = true;
                        result.IsLazyLoaded = true;
                    }
                }
            }
            
            return result;
        }

        private void AddOverlayStyles(IPublishedElement settings, string mainClass, Guid componentId, string overlayClass)
        {
            var overlayBgValue = settings.Value<string>("overlayBg");
            var cssBuilder = new StringBuilder();
            
            // ВАЖНО: используем отдельный элемент div для оверлея, а не псевдоэлемент
            cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 1; /* Выше видео (0), ниже контента (2) */
    pointer-events: none;
}}");
            
            // Стили в зависимости от типа оверлея
            switch (overlayBgValue)
            {
                case "Цвет":
                    if (settings.HasProperty("colorOverlay") && settings.HasValue("colorOverlay"))
                    {
                        var color = settings.Value<string>("colorOverlay");
                        if (!string.IsNullOrWhiteSpace(color))
                        {
                            cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    background-color: {color};
}}");
                        }
                    }
                    break;
                    
                case "Изображение":
                    if (settings.HasProperty("imageOverlay") && settings.HasValue("imageOverlay"))
                    {
                        var image = settings.Value<IPublishedContent>("imageOverlay");
                        if (image != null)
                        {
                            var imageUrl = image.Url();
                            cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    background-image: url('{imageUrl}');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}}");
                        }
                    }
                    break;
                    
                case "Градиент":
                    if (settings.HasProperty("colorStartOverlay") && settings.HasValue("colorStartOverlay") &&
                        settings.HasProperty("colorEndOverlay") && settings.HasValue("colorEndOverlay"))
                    {
                        var colorStart = settings.Value<string>("colorStartOverlay");
                        var colorEnd = settings.Value<string>("colorEndOverlay");
                        
                        if (!string.IsNullOrWhiteSpace(colorStart) && !string.IsNullOrWhiteSpace(colorEnd))
                        {
                            cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    background: linear-gradient(to bottom, {colorStart}, {colorEnd});
}}");
                        }
                    }
                    break;
            }
            
            // Прозрачность оверлея
            if (settings.HasProperty("opacityOverlay") && settings.HasValue("opacityOverlay"))
            {
                var opacityValue = settings.Value<int>("opacityOverlay");
                var opacity = opacityValue / 100.0;
                cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    opacity: {opacity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)};
}}");
            }
            
            AddToCssStyles(cssBuilder.ToString());
        }

        private void RegisterBackgroundInfo(IPublishedElement settings, Guid componentId, BackgroundResult result, string bgValue)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var backgroundInfos = httpContext.Items["LazyBackgroundsInfo"] as List<BackgroundInfo> ?? new List<BackgroundInfo>();
            
            var info = new BackgroundInfo
            {
                ComponentClass = result.CssClass,
                Type = bgValue,
                ComponentId = componentId.ToString()
            };
            
            switch (bgValue.Trim())
            {
                case "Изображение":
                    if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
                    {
                        var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                        if (bgImage != null)
                        {
                            info.Url = bgImage.Url();
                            info.Size = settings.HasValue("bgSize") ? 
                                ConvertBgSizeToCss(settings.Value<string>("bgSize") ?? "") : "cover";
                            info.Position = settings.HasValue("backgroundPosition") ? 
                                settings.Value<string>("backgroundPosition") ?? "center" : "center";
                        }
                    }
                    break;
                    
                case "Видео":
                    if (settings.HasProperty("video") && settings.HasValue("video"))
                    {
                        var videoUrl = settings.Value<string>("video");
                        if (!string.IsNullOrWhiteSpace(videoUrl))
                        {
                            var videoId = ExtractVimeoVideoId(videoUrl);
                            if (!string.IsNullOrEmpty(videoId))
                            {
                                info.VideoId = videoId;
                                
                                if (settings.HasProperty("videoPlaceholder") && settings.HasValue("videoPlaceholder"))
                                {
                                    var placeholder = settings.Value<IPublishedContent>("videoPlaceholder");
                                    if (placeholder != null)
                                    {
                                        info.PlaceholderUrl = placeholder.Url();
                                        info.UsePlaceholder = true;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            
            backgroundInfos.Add(info);
            httpContext.Items["LazyBackgroundsInfo"] = backgroundInfos;
        }

        private string ConvertBgSizeToCss(string bgSizeValue)
        {
            if (string.IsNullOrWhiteSpace(bgSizeValue))
                return "cover";
            
            var trimmedValue = bgSizeValue.Trim();
            
            return trimmedValue switch
            {
                "Как есть" => "auto",
                "По ширине" => "100% auto",
                "По высоте" => "auto 100%",
                "Обложка" => "cover",
                "Вместить" => "contain",
                _ => "cover"
            };
        }

        private string ConvertDirectionToCss(string directionValue)
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

        private string? ExtractVimeoVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            
            url = url.Split('?')[0];
            var regex = new System.Text.RegularExpressions.Regex(@"vimeo\.com/(?:.*/)?(\d+)");
            var match = regex.Match(url);
            
            return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;
        }

        private void AddToCssStyles(string css)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var cssStyles = httpContext.Items["BackgroundCss"] as List<string> ?? new List<string>();
            cssStyles.Add(css);
            httpContext.Items["BackgroundCss"] = cssStyles;
        }
    }

    // Класс для хранения информации о фонах
    public class BackgroundInfo
    {
        public string ComponentClass { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string ComponentId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Size { get; set; } = "cover";
        public string Position { get; set; } = "center";
        public string VideoId { get; set; } = string.Empty;
        public bool UsePlaceholder { get; set; }
        public string PlaceholderUrl { get; set; } = string.Empty;
    }
}