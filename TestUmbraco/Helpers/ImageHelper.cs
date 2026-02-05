using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Text;
using Umbraco.Cms.Core.Models.PublishedContent;
using TestUmbraco.Services;
using TestUmbraco.Models.Configuration;

namespace TestUmbraco.Helpers
{
    public class ImageHelper
    {
        private readonly IMediaCacheService _mediaCacheService;
        private readonly ImageOptimizationSettings? _settings;

        public ImageHelper(
            IMediaCacheService mediaCacheService,
            IOptions<ImageOptimizationSettings>? settings)
        {
            _mediaCacheService = mediaCacheService;
            _settings = settings?.Value;
        }

        public async Task<IHtmlContent> RenderMediaAsync(
            IPublishedContent? media,
            string? cropAlias = null,
            Dictionary<string, string>? attributes = null,
            bool lazyLoad = true)
        {
            if (media == null)
            {
                return new HtmlString(string.Empty);
            }
            
            try
            {
                var result = await _mediaCacheService.GetCachedImageHtmlAsync(media, cropAlias, attributes);
                return result ?? new HtmlString(string.Empty);
            }
            catch (Exception)
            {
                return new HtmlString(string.Empty);
            }
        }

        /// <summary>
        /// Рендерит адаптивное изображение с поддержкой srcset и picture
        /// </summary>
        public async Task<IHtmlContent> RenderResponsiveMediaAsync(
            IPublishedContent? media,
            string? cropAlias = null,
            string? cssClass = null,
            string? altText = null,
            bool lazyLoad = true,
            Dictionary<string, string>? attributes = null,
            int[]? responsiveWidths = null,
            string? sizes = null,
            string? gridColumns = "col-4",
            bool includeWebp = true,
            bool skipWrapper = false) // Новый параметр для отключения обертки
        {
            if (media == null)
            {
                return await RenderPlaceholderAsync(cssClass ?? string.Empty, skipWrapper);
            }

            try
            {
                // Используем настройки по умолчанию или переданные параметры
                var widths = responsiveWidths ?? _settings?.DefaultWidths ?? new[] { 400, 600, 800, 1000, 1200, 1920 };
                
                // Генерируем sizes на основе gridColumns если не передано явно
                var calculatedSizes = string.IsNullOrEmpty(sizes) 
                    ? GenerateSizesFromGrid(gridColumns ?? "col-12") 
                    : sizes;

                // Получаем базовый URL медиа
                var baseUrl = media.Url() ?? string.Empty;
                var hasCrop = !string.IsNullOrEmpty(cropAlias);

                // Создаем тег picture
                var pictureTag = new TagBuilder("picture");
                pictureTag.AddCssClass("responsive-image");

                // 1. Добавляем source для WebP (если включено)
                if (includeWebp && (_settings?.EnableWebp != false))
                {
                    var webpSource = new TagBuilder("source");
                    webpSource.Attributes["type"] = "image/webp";
                    webpSource.Attributes["srcset"] = GenerateSrcSet(baseUrl, widths, cropAlias, "webp") ?? string.Empty;
                    webpSource.Attributes["sizes"] = calculatedSizes ?? string.Empty;
                    pictureTag.InnerHtml.AppendHtml(webpSource);
                }

                // 2. Добавляем source для JPEG/PNG (fallback)
                var fallbackSource = new TagBuilder("source");
                fallbackSource.Attributes["srcset"] = GenerateSrcSet(baseUrl, widths, cropAlias, null) ?? string.Empty;
                fallbackSource.Attributes["sizes"] = calculatedSizes ?? string.Empty;
                pictureTag.InnerHtml.AppendHtml(fallbackSource);

                // 3. Добавляем основной img тег (fallback для старых браузеров)
                var imgTag = new TagBuilder("img");
                
                // Берем средний размер для fallback src
                var fallbackWidth = GetOptimalFallbackWidth(widths);
                var fallbackUrl = GenerateImageUrl(baseUrl, fallbackWidth, cropAlias, null);
                imgTag.Attributes["src"] = fallbackUrl ?? string.Empty;
                
                // Добавляем srcset для браузеров, которые поддерживают srcset но не picture
                imgTag.Attributes["srcset"] = GenerateSrcSet(baseUrl, widths, cropAlias, null) ?? string.Empty;
                imgTag.Attributes["sizes"] = calculatedSizes ?? string.Empty;
                
                // Базовые атрибуты
                imgTag.Attributes["alt"] = altText ?? media.Name ?? string.Empty;
                imgTag.Attributes["loading"] = lazyLoad ? "lazy" : "eager";
                imgTag.Attributes["decoding"] = "async";
                
                // Добавляем CSS классы
                var combinedCssClass = $"optimized-image {cssClass}".Trim();
                if (!string.IsNullOrEmpty(combinedCssClass))
                {
                    imgTag.AddCssClass(combinedCssClass);
                }
                
                // Добавляем дополнительные атрибуты
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        if (!string.IsNullOrEmpty(attr.Value))
                        {
                            imgTag.Attributes[attr.Key] = attr.Value;
                        }
                    }
                }

                pictureTag.InnerHtml.AppendHtml(imgTag);

                // Оборачиваем в div с классом image-wrapper, если skipWrapper = false
                if (!skipWrapper)
                {
                    var wrapper = new TagBuilder("div");
                    var wrapperClass = $"image-wrapper {cssClass}".Trim();
                    if (!string.IsNullOrEmpty(wrapperClass))
                    {
                        wrapper.AddCssClass(wrapperClass);
                    }
                    wrapper.InnerHtml.AppendHtml(pictureTag);
                    return new HtmlString(wrapper.ToHtmlString());
                }
                else
                {
                    // Возвращаем только picture без обертки
                    return new HtmlString(pictureTag.ToHtmlString());
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Error rendering responsive image: {ex.Message}");
                return await RenderFallbackAsync(media, cssClass ?? string.Empty, altText, lazyLoad, skipWrapper);
            }
        }

        /// <summary>
        /// Генерирует URL изображения с параметрами
        /// </summary>
        private string? GenerateImageUrl(string baseUrl, int width, string? cropAlias, string? format)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return null;

            var queryParams = new Dictionary<string, string>
            {
                ["width"] = width.ToString()
            };

            if (!string.IsNullOrEmpty(cropAlias))
            {
                queryParams["crop"] = cropAlias;
            }

            if (!string.IsNullOrEmpty(format))
            {
                queryParams["format"] = format;
                if (format == "webp")
                {
                    queryParams["quality"] = (_settings?.WebpQuality ?? 80).ToString();
                }
                else
                {
                    queryParams["quality"] = (_settings?.JpegQuality ?? 85).ToString();
                }
            }

            return AppendQueryParams(baseUrl, queryParams);
        }

        /// <summary>
        /// Генерирует строку srcset для указанных ширин
        /// </summary>
        private string? GenerateSrcSet(string baseUrl, int[] widths, string? cropAlias, string? format)
        {
            if (string.IsNullOrEmpty(baseUrl))
                return null;

            var srcsetParts = new List<string>();
            
            foreach (var width in widths)
            {
                var url = GenerateImageUrl(baseUrl, width, cropAlias, format);
                if (url != null)
                {
                    srcsetParts.Add($"{url} {width}w");
                }
            }
            
            return srcsetParts.Any() ? string.Join(", ", srcsetParts) : null;
        }

        /// <summary>
        /// Генерирует sizes на основе класса Bootstrap grid
        /// </summary>
        private string GenerateSizesFromGrid(string gridColumns)
        {
            if (string.IsNullOrEmpty(gridColumns))
                return "100vw";

            // Парсим количество колонок из строки типа "col-4", "col-md-6", etc.
            var colValue = ParseGridColumns(gridColumns);
            var fraction = colValue / 12.0;

            // Брейкпоинты Bootstrap по умолчанию
            var breakpoints = new Dictionary<int, double>
            {
                { 1920, 1920 * fraction },
                { 1400, 1400 * fraction },
                { 1200, 1200 * fraction },
                { 992, 992 * fraction },
                { 768, 768 * fraction },
                { 576, 576 * fraction }
            };

            // Генерируем sizes с учетом брейкпоинтов
            var sizesParts = new List<string>();
            foreach (var bp in breakpoints.OrderByDescending(b => b.Key))
            {
                sizesParts.Add($"(min-width: {bp.Key}px) {Math.Floor(bp.Value)}px");
            }
            sizesParts.Add("100vw");

            return string.Join(", ", sizesParts);
        }

        /// <summary>
        /// Парсит классы Bootstrap grid для получения количества колонок
        /// </summary>
        private int ParseGridColumns(string gridClass)
        {
            if (string.IsNullOrEmpty(gridClass))
                return 12;

            // Ищем паттерны: col-{n}, col-sm-{n}, col-md-{n}, col-lg-{n}, col-xl-{n}, col-xxl-{n}
            var patterns = new[] { "col-xxl-", "col-xl-", "col-lg-", "col-md-", "col-sm-", "col-" };
            
            foreach (var pattern in patterns)
            {
                if (gridClass.Contains(pattern))
                {
                    var startIndex = gridClass.IndexOf(pattern) + pattern.Length;
                    var remaining = gridClass.Substring(startIndex);
                    var numberString = new string(remaining.TakeWhile(char.IsDigit).ToArray());
                    
                    if (int.TryParse(numberString, out int result))
                    {
                        return result;
                    }
                }
            }

            return 12; // По умолчанию 12 колонок (полная ширина)
        }

        /// <summary>
        /// Выбирает оптимальную ширину для fallback изображения
        /// </summary>
        private int GetOptimalFallbackWidth(int[] widths)
        {
            // Берем предпоследний размер (например, для [400,600,800,1000,1200,1920] берем 1000)
            var index = Math.Max(0, widths.Length - 2);
            return widths[index];
        }

        /// <summary>
        /// Добавляет query-параметры к URL
        /// </summary>
        private string AppendQueryParams(string url, Dictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.Any())
                return url;

            var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            return $"{url}?{queryString}";
        }

        /// <summary>
        /// Рендерит placeholder при отсутствии изображения
        /// </summary>
        private Task<IHtmlContent> RenderPlaceholderAsync(string cssClass = "", bool skipWrapper = false)
        {
            string placeholder;
            
            if (!skipWrapper)
            {
                placeholder = $@"
                    <div class='image-placeholder {cssClass}'>
                        <div class='placeholder-content'>
                            <svg class='placeholder-icon' width='40' height='40' viewBox='0 0 24 24'>
                                <path fill='#adb5bd' d='M21,3H3C1.9,3,1,3.9,1,5v14c0,1.1,0.9,2,2,2h18c1.1,0,2-0.9,2-2V5C23,3.9,22.1,3,21,3z M21,19H3V5h18V19z'/>
                                <circle fill='#adb5bd' cx='9' cy='10' r='3'/>
                                <path fill='#adb5bd' d='M19,7h-6v6h6V7z'/>
                            </svg>
                            <span class='placeholder-text'>Нет изображения</span>
                        </div>
                    </div>
                ";
            }
            else
            {
                placeholder = $@"
                    <div class='placeholder-content'>
                        <svg class='placeholder-icon' width='40' height='40' viewBox='0 0 24 24'>
                            <path fill='#adb5bd' d='M21,3H3C1.9,3,1,3.9,1,5v14c0,1.1,0.9,2,2,2h18c1.1,0,2-0.9,2-2V5C23,3.9,22.1,3,21,3z M21,19H3V5h18V19z'/>
                            <circle fill='#adb5bd' cx='9' cy='10' r='3'/>
                            <path fill='#adb5bd' d='M19,7h-6v6h6V7z'/>
                        </svg>
                        <span class='placeholder-text'>Нет изображения</span>
                    </div>
                ";
            }

            return Task.FromResult<IHtmlContent>(new HtmlString(placeholder));
        }

        /// <summary>
        /// Рендерит fallback изображение при ошибке
        /// </summary>
        private Task<IHtmlContent> RenderFallbackAsync(
            IPublishedContent media, 
            string cssClass, 
            string? altText, 
            bool lazyLoad,
            bool skipWrapper = false)
        {
            string fallback;
            
            if (!skipWrapper)
            {
                fallback = $@"
                    <div class='image-wrapper {cssClass}'>
                        <img src='{media.Url()}' 
                             alt='{altText ?? media.Name ?? string.Empty}'
                             class='image-fallback {cssClass}'
                             loading='{(lazyLoad ? "lazy" : "eager")}'
                             decoding='async' />
                    </div>
                ";
            }
            else
            {
                fallback = $@"
                    <img src='{media.Url()}' 
                         alt='{altText ?? media.Name ?? string.Empty}'
                         class='image-fallback {cssClass}'
                         loading='{(lazyLoad ? "lazy" : "eager")}'
                         decoding='async' />
                ";
            }

            return Task.FromResult<IHtmlContent>(new HtmlString(fallback));
        }

        /// <summary>
        /// Упрощенный метод для обратной совместимости
        /// </summary>
        public async Task<IHtmlContent> RenderResponsiveImageAsync(
            IPublishedContent? media,
            string? cropAlias = null,
            string? cssClass = null,
            string? altText = null,
            bool lazyLoad = true,
            string? gridColumns = "col-4")
        {
            return await RenderResponsiveMediaAsync(
                media: media,
                cropAlias: cropAlias,
                cssClass: cssClass,
                altText: altText,
                lazyLoad: lazyLoad,
                gridColumns: gridColumns
            );
        }
        
        /// <summary>
        /// Упрощенный метод для обратной совместимости с skipWrapper
        /// </summary>
        public async Task<IHtmlContent> RenderResponsiveImageAsync(
            IPublishedContent? media,
            string? cropAlias = null,
            string? cssClass = null,
            string? altText = null,
            bool lazyLoad = true,
            string? gridColumns = "col-4",
            bool skipWrapper = false)
        {
            return await RenderResponsiveMediaAsync(
                media: media,
                cropAlias: cropAlias,
                cssClass: cssClass,
                altText: altText,
                lazyLoad: lazyLoad,
                gridColumns: gridColumns,
                skipWrapper: skipWrapper
            );
        }
    }
}