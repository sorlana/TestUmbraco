using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public class UmbracoBackgroundService : IUmbracoBackgroundService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UmbracoBackgroundService> _logger;
        private readonly IMediaCacheService _mediaCacheService;
        private readonly IStaticCssGeneratorService _staticCssGenerator;

        public UmbracoBackgroundService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<UmbracoBackgroundService> logger,
            IMediaCacheService mediaCacheService,
            IStaticCssGeneratorService staticCssGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mediaCacheService = mediaCacheService;
            _staticCssGenerator = staticCssGenerator;
        }

        public async Task<BackgroundResult> ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg")
        {
            var result = new BackgroundResult();
            
            if (settings == null) return result;

            if (settings.HasProperty("bg") && settings.HasValue("bg"))
            {
                var bgValue = settings.Value<string>("bg");
                if (!string.IsNullOrWhiteSpace(bgValue))
                {
                    result = await ProcessBackgroundType(settings, componentId, prefix, bgValue);
                    
                    // Обработка оверлея
                    if (result.HasBackground && settings.HasProperty("overlayBg") && settings.HasValue("overlayBg"))
                    {
                        var overlayBgValue = settings.Value<string>("overlayBg");
                        if (!string.IsNullOrWhiteSpace(overlayBgValue) && overlayBgValue != "Не выбрано" && overlayBgValue != "None")
                        {
                            result.HasOverlay = true;
                            result.OverlayClass = $"overlay-{componentId:N}";
                            result.CssClass += $" {result.OverlayClass}";
                            await AddOverlayStyles(settings, componentId, result.OverlayClass);
                        }
                    }
                    
                    // Регистрируем информацию для JavaScript
                    RegisterBackgroundInfo(settings, componentId, result, bgValue);
                }
            }
            
            return result;
        }

        private async Task<BackgroundResult> ProcessBackgroundType(IPublishedElement settings, Guid componentId, string prefix, string bgValue)
        {
            BackgroundResult result;
            
            var trimmedValue = bgValue.Trim();
            
            switch (trimmedValue)
            {
                case "Изображение":
                    result = await ProcessImageBackground(settings, componentId, prefix);
                    break;
                case "Цвет":
                    result = await ProcessColorBackground(settings, componentId, prefix);
                    break;
                case "Градиент":
                    result = await ProcessGradientBackground(settings, componentId, prefix);
                    break;
                case "Видео":
                    result = await ProcessVideoBackground(settings, componentId, prefix);
                    break;
                default:
                    result = new BackgroundResult();
                    break;
            }
            
            return result;
        }

        private async Task<BackgroundResult> ProcessImageBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Image };
            
            if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
            {
                var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                if (bgImage != null)
                {
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    var bgSize = settings.HasValue("bgSize") ? 
                        ConvertBgSizeToCss(settings.Value<string>("bgSize") ?? "") : "cover";
                    var bgPosition = settings.HasValue("backgroundPosition") ? 
                        settings.Value<string>("backgroundPosition") ?? "center" : "center";
                    
                    // Генерируем класс через статический CSS сервис
                    var className = $"bg-media-{bgImage.Key:N}";
                    if (bgSize == "contain")
                    {
                        className += "-contain";
                    }
                    
                    // Добавляем стиль в статический CSS файл
                    await _staticCssGenerator.GetOrAddMediaClassAsync(
                        bgImage.Key, 
                        className, 
                        minHeight, 
                        bgSize, 
                        bgPosition);
                    
                    result.CssClass = $"{className} lazy-bg";
                    result.HasBackground = true;
                    result.IsLazyLoaded = true;
                    
                    // Добавляем инлайновые стили для min-height
                    if (minHeight > 0)
                    {
                        var minHeightClass = $"min-h-{minHeight}";
                        await _staticCssGenerator.AddInlineStyleAsync($"min-height: {minHeight}px;", "minheight");
                        result.CssClass += $" {minHeightClass}";
                    }
                }
            }
            
            return result;
        }

        private async Task<BackgroundResult> ProcessColorBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Color };
            
            if (settings.HasProperty("color") && settings.HasValue("color"))
            {
                var color = settings.Value<string>("color");
                if (!string.IsNullOrWhiteSpace(color))
                {
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    // Генерируем класс через статический CSS сервис
                    var className = await _staticCssGenerator.GetOrAddColorClassAsync(color, minHeight);
                    
                    result.CssClass = className;
                    result.HasBackground = true;
                }
            }
            
            return result;
        }

        private async Task<BackgroundResult> ProcessGradientBackground(IPublishedElement settings, Guid componentId, string prefix)
        {
            var result = new BackgroundResult { Type = BackgroundType.Gradient };
            
            if (settings.HasProperty("colorStart") && settings.HasValue("colorStart") &&
                settings.HasProperty("colorEnd") && settings.HasValue("colorEnd"))
            {
                var colorStart = settings.Value<string>("colorStart");
                var colorEnd = settings.Value<string>("colorEnd");
                
                if (!string.IsNullOrWhiteSpace(colorStart) && !string.IsNullOrWhiteSpace(colorEnd))
                {
                    var direction = "to bottom";
                    if (settings.HasProperty("direction") && settings.HasValue("direction"))
                    {
                        direction = ConvertDirectionToCss(settings.Value<string>("direction") ?? "");
                    }
                    
                    var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                    
                    // Генерируем класс через статический CSS сервис
                    var className = await _staticCssGenerator.GetOrAddGradientClassAsync(
                        colorStart, colorEnd, direction, minHeight);
                    
                    result.CssClass = className;
                    result.HasBackground = true;
                }
            }
            
            return result;
        }

        private async Task<BackgroundResult> ProcessVideoBackground(IPublishedElement settings, Guid componentId, string prefix)
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
                        result.VideoId = videoId;
                        
                        var minHeight = settings.HasValue("minHeight") ? settings.Value<int>("minHeight") : 400;
                        
                        // Генерируем уникальный класс для видео
                        var videoHash = ComputeHash(videoUrl);
                        var videoClass = $"bg-video-{videoHash}";
                        
                        // Добавляем стили в статический CSS
                        var css = $@"
.{videoClass}.lazy-video {{
    position: relative;
    min-height: {minHeight}px;
    overflow: hidden;
}}

.{videoClass}.lazy-video .video-container {{
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 0;
    pointer-events: none;
}}

.{videoClass}.lazy-video .video-bg-iframe {{
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
}}

.{videoClass}.lazy-video .video-placeholder {{
    background-size: cover;
    background-position: center;
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}}";
                        
                        await _staticCssGenerator.AddInlineStyleAsync(css, "video");
                        
                        result.CssClass = $"{videoClass} lazy-video";
                        result.HasBackground = true;
                        result.IsLazyLoaded = true;
                        
                        // Проверяем наличие плейсхолдера
                        if (settings.HasProperty("videoPlaceholder") && settings.HasValue("videoPlaceholder"))
                        {
                            var placeholder = settings.Value<IPublishedContent>("videoPlaceholder");
                            if (placeholder != null)
                            {
                                var placeholderUrl = await _mediaCacheService.GetCachedMediaUrlAsync(placeholder.Key);
                                if (!string.IsNullOrEmpty(placeholderUrl))
                                {
                                    result.VideoPlaceholder = placeholderUrl;
                                    result.UseVideoPlaceholder = true;
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        private async Task AddOverlayStyles(IPublishedElement settings, Guid componentId, string overlayClass)
        {
            var overlayBgValue = settings.Value<string>("overlayBg");
            
            var cssBuilder = new StringBuilder();
            
            cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 1;
    pointer-events: none;
}}");
            
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
                            var imageUrl = await _mediaCacheService.GetCachedMediaUrlAsync(image.Key);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                cssBuilder.Append($@"
.{overlayClass} .background-overlay {{
    background-image: url('{imageUrl}');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
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
            
            await _staticCssGenerator.AddInlineStyleAsync(cssBuilder.ToString(), "overlay");
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
                ComponentId = componentId.ToString(),
                HasOverlay = result.HasOverlay,
                OverlayClass = result.OverlayClass
            };
            
            switch (bgValue.Trim())
            {
                case "Изображение":
                    if (settings.HasProperty("backgroundImage") && settings.HasValue("backgroundImage"))
                    {
                        var bgImage = settings.Value<IPublishedContent>("backgroundImage");
                        if (bgImage != null)
                        {
                            info.Url = _mediaCacheService.GetCachedMediaUrlAsync(bgImage.Key).GetAwaiter().GetResult() ?? string.Empty;
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
                                        info.PlaceholderUrl = _mediaCacheService.GetCachedMediaUrlAsync(placeholder.Key).GetAwaiter().GetResult() ?? string.Empty;
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
            var regex = new Regex(@"vimeo\.com/(?:.*/)?(\d+)");
            var match = regex.Match(url);
            
            return match.Success && match.Groups.Count > 1 ? match.Groups[1].Value : null;
        }

        private string ComputeHash(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 8);
        }
    }
}