using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public class UmbracoBackgroundService : IUmbracoBackgroundService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggingService _loggingService;
        private readonly IMediaCacheService _mediaCacheService;
        private readonly IStaticCssGeneratorService _staticCssGenerator;

        public UmbracoBackgroundService(
            IHttpContextAccessor httpContextAccessor,
            ILoggingService loggingService,
            IMediaCacheService mediaCacheService,
            IStaticCssGeneratorService staticCssGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _loggingService = loggingService;
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
                        if (!string.IsNullOrWhiteSpace(overlayBgValue) && 
                            overlayBgValue != "Не выбран" && overlayBgValue != "None" && 
                            overlayBgValue != "мЕ БШАПЮМ")
                        {
                            result.HasOverlay = true;
                            result.OverlayClass = $"overlay-{componentId:N}";
                            result.CssClass += $" {result.OverlayClass}";
                            await AddOverlayStyles(settings, componentId, result.OverlayClass);
                        }
                    }
                    
                    // Передача данных для JavaScript
                    RegisterBackgroundInfo(settings, componentId, result, bgValue);
                }
            }
            
            return result;
        }

        private async Task<BackgroundResult> ProcessBackgroundType(IPublishedElement settings, Guid componentId, string prefix, string bgValue)
        {
            BackgroundResult result;
            
            var trimmedValue = bgValue.Trim();
            
            // Поддержка как русских, так и английских значений
            if (trimmedValue == "Изображение" || trimmedValue == "Image" || trimmedValue == "хГНАПЮФЕМХЕ")
            {
                result = await ProcessImageBackground(settings, componentId, prefix);
            }
            else if (trimmedValue == "Цвет" || trimmedValue == "Color" || trimmedValue == "жБЕР")
            {
                result = await ProcessColorBackground(settings, componentId, prefix);
            }
            else if (trimmedValue == "Градиент" || trimmedValue == "Gradient" || trimmedValue == "цПЮДХЕМР")
            {
                result = await ProcessGradientBackground(settings, componentId, prefix);
            }
            else if (trimmedValue == "Видео" || trimmedValue == "Video" || trimmedValue == "бХДЕН")
            {
                result = await ProcessVideoBackground(settings, componentId, prefix);
            }
            else
            {
                result = new BackgroundResult();
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
                    
                    // Генерация класса с медиа
                    var className = $"bg-media-{bgImage.Key:N}";
                    if (bgSize == "contain")
                    {
                        className += "-contain";
                    }
                    
                    // Добавляем стили в CSS
                    await _staticCssGenerator.GetOrAddMediaClassAsync(
                        bgImage.Key, 
                        className, 
                        minHeight, 
                        bgSize, 
                        bgPosition);
                    
                    result.CssClass = $"{className} lazy-bg";
                    result.HasBackground = true;
                    result.IsLazyLoaded = true;
                    
                    // Минимальную высоту НЕ добавляем - высота по содержимому
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
                    var minHeight = 0; // Убираем min-height
                    
                    // Генерация класса с цветом
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
                    
                    var minHeight = 0; // Убираем min-height
                    
                    // Генерация класса с градиентом
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
                        
                        // Убираем min-height
                        
                        // Генерация уникального класса для видео
                        var videoHash = ComputeHash(videoUrl);
                        var videoClass = $"bg-video-{videoHash}";
                        
                        // Добавляем стили в CSS (без min-height)
                        var css = $@"
.{videoClass}.lazy-video {{
    position: relative;
    overflow: hidden;
}}

.{videoClass}.lazy-video .video-container {{
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -2;
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
    z-index: -1;
    pointer-events: none;
}}

/* Overlay должен быть над видео, но под контентом */
.{videoClass}.lazy-video::before {{
    z-index: 0 !important;
}}";
                        
                        await _staticCssGenerator.AddInlineStyleAsync(css, "video");
                        
                        result.CssClass = $"{videoClass} lazy-video";
                        result.HasBackground = true;
                        result.IsLazyLoaded = true;
                        
                        // Проверяем наличие видеоплейсхолдера
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
            
            // Определяем z-index в зависимости от типа фона
            // Если есть видео: z-index: 0 (над видео, но под контентом)
            // Если фоновое изображение/цвет/градиент: z-index: 1 (над фоном, под контентом)
            var bgType = settings.Value<string>("bg")?.Trim();
            var hasVideo = bgType == "Видео" || bgType == "Video" || bgType == "бХДЕН";
            var overlayZIndex = hasVideo ? 0 : 1;
            var contentZIndex = hasVideo ? 1 : 2;
            
            _loggingService.LogInformation<UmbracoBackgroundService>(
                $"AddOverlayStyles: componentId={componentId}, bgType={bgType}, hasVideo={hasVideo}, overlayZIndex={overlayZIndex}");
            
            // Отладка: выводим все свойства settings
            if (settings != null)
            {
                var props = settings.Properties.Select(p => 
                    $"{p.Alias}={p.GetValue()}").ToList();
                _loggingService.LogInformation<UmbracoBackgroundService>(
                    $"Settings properties: {string.Join(", ", props)}");
            }
            
            // Добавляем основу: оверлей должен быть под контентом, но над фоном/видео
            cssBuilder.Append($@"
.{overlayClass} {{
    position: relative;
}}

/* Оверлей над фоном/видео, но под контентом */
.{overlayClass}::before {{
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: {overlayZIndex};
    pointer-events: none;
}}");
            
            // Настраиваем тип оверлея
            if (overlayBgValue == "Цвет" || overlayBgValue == "Color" || overlayBgValue == "жБЕР")
            {
                if (settings != null && settings.HasProperty("colorOverlay") && settings.HasValue("colorOverlay"))
                {
                    var color = settings.Value<string>("colorOverlay");
                    if (!string.IsNullOrWhiteSpace(color))
                    {
                        cssBuilder.Append($@"
.{overlayClass}::before {{
    background-color: {color};
}}");
                    }
                }
            }
            else if (overlayBgValue == "Изображение" || overlayBgValue == "Image" || overlayBgValue == "хГНАПЮФЕМХЕ")
            {
                if (settings != null && settings.HasProperty("imageOverlay") && settings.HasValue("imageOverlay"))
                {
                    var image = settings.Value<IPublishedContent>("imageOverlay");
                    if (image != null)
                    {
                        var imageUrl = await _mediaCacheService.GetCachedMediaUrlAsync(image.Key);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            cssBuilder.Append($@"
.{overlayClass}::before {{
    background-image: url('{imageUrl}');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
}}");
                        }
                    }
                }
            }
            else if (overlayBgValue == "Градиент" || overlayBgValue == "Gradient" || overlayBgValue == "цПЮДХЕМР")
            {
                if (settings != null && settings.HasProperty("colorStartOverlay") && settings.HasValue("colorStartOverlay") &&
                    settings.HasProperty("colorEndOverlay") && settings.HasValue("colorEndOverlay"))
                {
                    var colorStart = settings.Value<string>("colorStartOverlay");
                    var colorEnd = settings.Value<string>("colorEndOverlay");
                    
                    if (!string.IsNullOrWhiteSpace(colorStart) && !string.IsNullOrWhiteSpace(colorEnd))
                    {
                        // Определяем направление градиента - проверяем оба возможных имени свойства
                        var direction = "to bottom";
                        if (settings.HasProperty("directionOverlay") && settings.HasValue("directionOverlay"))
                        {
                            direction = ConvertDirectionToCss(settings.Value<string>("directionOverlay") ?? "");
                        }
                        else if (settings.HasProperty("directionGradient") && settings.HasValue("directionGradient"))
                        {
                            direction = ConvertDirectionToCss(settings.Value<string>("directionGradient") ?? "");
                        }
                        
                        cssBuilder.Append($@"
.{overlayClass}::before {{
    background: linear-gradient({direction}, {colorStart}, {colorEnd});
}}");
                    }
                }
            }
            
            // Прозрачность оверлея
            _loggingService.LogInformation<UmbracoBackgroundService>(
                $"Checking opacity: HasProperty(opacityOverlay)={settings?.HasProperty("opacityOverlay")}, HasValue={settings?.HasValue("opacityOverlay")}");
            
            if (settings != null && settings.HasProperty("opacityOverlay"))
            {
                // Логируем сырое значение
                var rawValue = settings.Value("opacityOverlay");
                _loggingService.LogInformation<UmbracoBackgroundService>(
                    $"Overlay opacity RAW: value={rawValue}, type={rawValue?.GetType().Name ?? "null"}");
                
                // Пробуем разные типы данных
                int opacityValue = 0;
                
                try
                {
                    opacityValue = settings.Value<int>("opacityOverlay");
                }
                catch
                {
                    try
                    {
                        opacityValue = (int)settings.Value<decimal>("opacityOverlay");
                    }
                    catch
                    {
                        try
                        {
                            var strValue = settings.Value<string>("opacityOverlay");
                            int.TryParse(strValue, out opacityValue);
                        }
                        catch
                        {
                            opacityValue = 0;
                        }
                    }
                }
                
                // Если значение 0, НЕ генерируем opacity вообще
                // Это позволит использовать старые значения из CSS
                if (opacityValue > 0)
                {
                    var opacity = opacityValue / 100.0m;
                    
                    _loggingService.LogInformation<UmbracoBackgroundService>(
                        $"Overlay opacity: opacityValue={opacityValue}, opacity={opacity}, overlayClass={overlayClass}");
                    
                    cssBuilder.Append($@"
.{overlayClass}::before {{
    opacity: {opacity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)};
}}");
                }
                else
                {
                    _loggingService.LogWarning<UmbracoBackgroundService>(
                        $"Overlay opacity is 0, skipping opacity generation for {overlayClass}");
                }
            }
            else
            {
                _loggingService.LogWarning<UmbracoBackgroundService>(
                    $"Overlay opacity NOT found: HasProperty={settings.HasProperty("opacityOverlay")}, overlayClass={overlayClass}");
            }
            
            // Добавляем стили для правильного позиционирования контента над оверлеем
            cssBuilder.Append($@"
/* Позиционируем контент над оверлеем */
.{overlayClass} > .container,
.{overlayClass} > [class*=""container""] {{
    position: relative;
    z-index: {contentZIndex};
}}

/* Все прямые дочерние элементы, кроме video-container */
.{overlayClass} > *:not(.video-container) {{
    position: relative;
    z-index: {contentZIndex};
}}");
            
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
            
            var trimmedBgValue = bgValue.Trim();
            
            if (trimmedBgValue == "Изображение" || trimmedBgValue == "Image" || trimmedBgValue == "хГНАПЮФЕМХЕ")
            {
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
            }
            else if (trimmedBgValue == "Видео" || trimmedBgValue == "Video" || trimmedBgValue == "бХДЕН")
            {
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
            }
            
            backgroundInfos.Add(info);
            httpContext.Items["LazyBackgroundsInfo"] = backgroundInfos;
        }

        private string ConvertBgSizeToCss(string bgSizeValue)
        {
            if (string.IsNullOrWhiteSpace(bgSizeValue))
                return "cover";
            
            var trimmedValue = bgSizeValue.Trim();
            
            if (trimmedValue == "йЮЙ ЕЯРЭ")
                return "auto";
            if (trimmedValue == "оН ЬХПХМЕ")
                return "100% auto";
            if (trimmedValue == "оН БШЯНРЕ")
                return "auto 100%";
            if (trimmedValue == "нАКНФЙЮ")
                return "cover";
            if (trimmedValue == "бЛЕЯРХРЭ")
                return "contain";
            
            return "cover";
        }

        private string ConvertDirectionToCss(string directionValue)
        {
            if (string.IsNullOrWhiteSpace(directionValue))
                return "to bottom";
            
            var trimmedValue = directionValue.Trim();
            
            // Поддержка русских и английских значений
            if (trimmedValue == "Сверху вниз" || trimmedValue == "Top to Bottom" || trimmedValue == "яБЕПУС БМХГ") return "to bottom";
            if (trimmedValue == "Снизу вверх" || trimmedValue == "Bottom to Top" || trimmedValue == "яМХГС ББЕПУ") return "to top";
            if (trimmedValue == "Слева направо" || trimmedValue == "Left to Right" || trimmedValue == "яКЕБЮ МЮОПЮБН") return "to right";
            if (trimmedValue == "Справа налево" || trimmedValue == "Right to Left" || trimmedValue == "яОПЮБЮ МЮКЕБН") return "to left";
            
            // Обработка диагональных направлений
            if (trimmedValue.Contains("Диагональ") || trimmedValue == "дХЮЦНМЮКЭ")
            {
                // Определяем конкретное направление по содержимому строки
                if (trimmedValue.Contains("вниз") && trimmedValue.Contains("право"))
                    return "to bottom right";
                if (trimmedValue.Contains("вниз") && trimmedValue.Contains("лево"))
                    return "to bottom left";
                if (trimmedValue.Contains("вверх") && trimmedValue.Contains("право"))
                    return "to top right";
                if (trimmedValue.Contains("вверх") && trimmedValue.Contains("лево"))
                    return "to top left";
                    
                // Старая кодировка
                if (trimmedValue.Contains("БМХГ") && trimmedValue.Contains("ОПЮБН"))
                    return "to bottom right";
                if (trimmedValue.Contains("БМХГ") && trimmedValue.Contains("КЕБН"))
                    return "to bottom left";
                if (trimmedValue.Contains("ББЕПУ") && trimmedValue.Contains("ОПЮБН"))
                    return "to top right";
                if (trimmedValue.Contains("ББЕПУ") && trimmedValue.Contains("КЕБН"))
                    return "to top left";
            }
            
            return "to bottom";
        }

        private string? ExtractVimeoVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            
            // Если это уже просто ID (только цифры), возвращаем как есть
            if (System.Text.RegularExpressions.Regex.IsMatch(url.Trim(), @"^\d+$"))
            {
                return url.Trim();
            }
            
            // Иначе пытаемся извлечь из URL
            url = url.Split('?')[0];
            var regex = new Regex(@"vimeo\.com/(?:.*/)?(\d+)");
            var match = regex.Match(url);
            
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            
            return null;
        }

        private string ComputeHash(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 8);
        }
    }
}