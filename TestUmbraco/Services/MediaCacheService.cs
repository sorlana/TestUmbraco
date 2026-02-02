using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Microsoft.AspNetCore.Hosting;
using System.Text;

#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL

namespace TestUmbraco.Services
{
    public class MediaCacheService : IMediaCacheService
    {
        private readonly IAppPolicyCache _runtimeCache;
        private readonly IMediaService _mediaService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILogger<MediaCacheService> _logger;
        private readonly IWebHostEnvironment _environment;

        public MediaCacheService(
            AppCaches appCaches,
            IMediaService mediaService,
            IUmbracoContextAccessor umbracoContextAccessor,
            ILogger<MediaCacheService> logger,
            IWebHostEnvironment environment)
        {
            _runtimeCache = appCaches.RuntimeCache;
            _mediaService = mediaService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
            _environment = environment;
        }

        public async Task<string?> GetCachedMediaUrlAsync(Guid mediaKey, string? cropAlias = null, int? width = null, int? height = null)
        {
            var cacheKey = $"media_url_{mediaKey}_{cropAlias}_{width}_{height}";
            
            return await _runtimeCache.GetCacheItemAsync(cacheKey, async () =>
            {
                try
                {
                    var media = _mediaService.GetById(mediaKey);
                    if (media == null) return null;

                    string? url;
                    
                    if (_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
                    {
                        var publishedMedia = umbracoContext.Media?.GetById(mediaKey);
                        url = publishedMedia?.Url();
                    }
                    else
                    {
                        url = media.GetValue<string>("umbracoFile");
                    }

                    if (string.IsNullOrEmpty(url)) return null;

                    // Добавляем параметры для ImageProcessor
                    var parameters = new List<string>();
                    
                    if (!string.IsNullOrEmpty(cropAlias))
                        parameters.Add($"crop={cropAlias}");
                    
                    if (width.HasValue)
                        parameters.Add($"width={width}");
                    
                    if (height.HasValue)
                        parameters.Add($"height={height}");
                    
                    // Версия для инвалидации кеша
                    var version = media.UpdateDate != default ? media.UpdateDate.Ticks.ToString() : DateTime.UtcNow.Ticks.ToString();
                    
                    if (parameters.Any())
                        url = $"{url}?{string.Join("&", parameters)}&v={version}";
                    else if (!url.Contains("?"))
                        url = $"{url}?v={version}";
                    else
                        url = $"{url}&v={version}";

                    return url;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting media URL for {mediaKey}");
                    return null;
                }
            }, TimeSpan.FromHours(1));
        }

        public async Task<string?> GetCachedMediaUrlAsync(IPublishedContent? media, string? cropAlias = null, int? width = null, int? height = null)
        {
            if (media == null) return null;
            return await GetCachedMediaUrlAsync(media.Key, cropAlias, width, height);
        }

        public async Task<IHtmlContent> GetCachedImageHtmlAsync(Guid mediaKey, string? cropAlias = null, Dictionary<string, string>? attributes = null)
        {
            var cacheKey = $"media_html_{mediaKey}_{cropAlias}_{(attributes != null ? string.Join("_", attributes) : "")}";
            
            return await _runtimeCache.GetCacheItemAsync(cacheKey, async () =>
            {
                try
                {
                    var media = _mediaService.GetById(mediaKey);
                    if (media == null) return new HtmlString(string.Empty);

                    var url = await GetCachedMediaUrlAsync(mediaKey, cropAlias);
                    if (string.IsNullOrEmpty(url)) return new HtmlString(string.Empty);

                    // Получаем дополнительные данные
                    string? altText = media.GetValue<string>("altText") ?? media.Name;
                    string? title = media.GetValue<string>("title") ?? string.Empty;
                    
                    // Генерируем HTML
                    var html = GeneratePictureElement(url, altText, title, 
                        media.GetValue<int?>("width"), 
                        media.GetValue<int?>("height"), 
                        true);

                    return new HtmlString(html);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error generating HTML for media {mediaKey}");
                    return new HtmlString(string.Empty);
                }
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<IHtmlContent> GetCachedImageHtmlAsync(IPublishedContent? media, string? cropAlias = null, Dictionary<string, string>? attributes = null)
        {
            if (media == null) return new HtmlString(string.Empty);
            return await GetCachedImageHtmlAsync(media.Key, cropAlias, attributes);
        }

        public async Task<string> GetCachedBackgroundCssAsync(string backgrounds)
        {
            var cacheKey = $"bg_css_{backgrounds.GetHashCode()}";
            
            var cachedValue = await _runtimeCache.GetCacheItemAsync(cacheKey, async () =>
            {
                var cssBuilder = new StringBuilder();
                var backgroundItems = backgrounds.Split('|', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var item in backgroundItems)
                {
                    var parts = item.Split(':');
                    if (parts.Length >= 2)
                    {
                        if (Guid.TryParse(parts[0], out Guid mediaGuid))
                        {
                            var className = parts[1];
                            var minHeight = parts.Length > 2 && int.TryParse(parts[2], out var mh) ? mh : 400;
                            var size = parts.Length > 3 ? parts[3] : "cover";
                            var position = parts.Length > 4 ? parts[4] : "center";
                            
                            var css = await GenerateBackgroundCssAsync(mediaGuid, className, minHeight, size, position);
                            if (!string.IsNullOrEmpty(css))
                                cssBuilder.AppendLine(css);
                        }
                    }
                }
                
                var result = cssBuilder.ToString();
                return result; // string.Empty если пусто
            }, TimeSpan.FromHours(1));

            // Гарантированно возвращаем не-null строку
            return cachedValue ?? string.Empty;
        }

        public async Task<string> GenerateBackgroundCssAsync(Guid mediaGuid, string className, int minHeight = 400, string size = "cover", string position = "center")
        {
            var url = await GetCachedMediaUrlAsync(mediaGuid);
            if (string.IsNullOrEmpty(url))
                return string.Empty;
            
            return $@".{className} {{
    background-image: url('{url}');
    background-size: {size};
    background-position: {position};
    background-repeat: no-repeat;
    min-height: {minHeight}px;
}}";
        }

        public void ClearCacheForMedia(Guid mediaKey)
        {
            // Очищаем все варианты кеша для этого медиа
            _runtimeCache.ClearByKey($"media_url_{mediaKey}");
            _runtimeCache.ClearByKey($"media_html_{mediaKey}");
            
            _logger.LogInformation($"Cleared cache for media: {mediaKey}");
        }

        public void ClearAllCache()
        {
            _runtimeCache.Clear();
            _logger.LogInformation("Cleared all media cache");
        }

        public string GeneratePictureElement(string? url, string? altText = "", string? title = "", int? width = null, int? height = null, bool lazyLoad = true)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            
            var webpUrl = ConvertToWebP(url);
            var srcset = width.HasValue ? GenerateSrcSet(url, null, width.Value) : string.Empty;
            
            var html = new StringBuilder();
            
            html.AppendLine("<picture>");
            html.AppendLine($"  <source srcset=\"{webpUrl}\" type=\"image/webp\">");
            
            if (!string.IsNullOrEmpty(srcset))
                html.AppendLine($"  <source srcset=\"{srcset}\">");
            
            html.Append($"  <img src=\"{url}\"");
            
            if (!string.IsNullOrEmpty(altText))
                html.Append($" alt=\"{altText}\"");
            
            if (!string.IsNullOrEmpty(title))
                html.Append($" title=\"{title}\"");
            
            if (lazyLoad)
                html.Append(" loading=\"lazy\" decoding=\"async\"");
            
            html.Append(" class=\"optimized-image\"");
            
            if (width.HasValue)
                html.Append($" width=\"{width}\"");
            
            if (height.HasValue)
                html.Append($" height=\"{height}\"");
            
            html.AppendLine(">");
            html.AppendLine("</picture>");
            
            return html.ToString();
        }

        public string ConvertToWebP(string? url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            
            if (url.Contains("?"))
                return $"{url}&format=webp";
            return $"{url}?format=webp";
        }

        public string GenerateSrcSet(string? url, string? cropAlias, int baseWidth)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;
            
            var srcset = new List<string>();
            var multipliers = new[] { 1, 1.5, 2, 3 };
            
            foreach (var multiplier in multipliers)
            {
                var width = (int)(baseWidth * multiplier);
                var resizedUrl = url.Contains("?") 
                    ? $"{url}&width={width}" 
                    : $"{url}?width={width}";
                srcset.Add($"{resizedUrl} {multiplier}x");
            }
            
            return string.Join(", ", srcset);
        }
    }
}