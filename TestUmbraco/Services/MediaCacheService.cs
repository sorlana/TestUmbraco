using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Microsoft.AspNetCore.Hosting;
using System.Text;

namespace TestUmbraco.Services
{
    public class MediaCacheService : IMediaCacheService
    {
        private readonly IAppPolicyCache _runtimeCache;
        private readonly IMediaService _mediaService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MediaCacheService(
            AppCaches appCaches,
            IMediaService mediaService,
            IUmbracoContextAccessor umbracoContextAccessor)
        {
            _runtimeCache = appCaches.RuntimeCache;
            _mediaService = mediaService;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public async Task<string?> GetCachedMediaUrlAsync(Guid mediaKey, string? cropAlias = null, int? width = null, int? height = null)
        {
            var cacheKey = $"media_url_{mediaKey}_{cropAlias}_{width}_{height}";
            
            return await _runtimeCache.GetCacheItemAsync(cacheKey, async () =>
            {
                try
                {
                    var media = _mediaService.GetById(mediaKey);
                    
                    if (media == null)
                    {
                        return null;
                    }

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

                    if (string.IsNullOrEmpty(url))
                    {
                        return null;
                    }

                    var parameters = new List<string>();
                    
                    if (!string.IsNullOrEmpty(cropAlias))
                    {
                        parameters.Add($"crop={cropAlias}");
                    }
                    
                    if (width.HasValue)
                    {
                        parameters.Add($"width={width}");
                    }
                    
                    if (height.HasValue)
                    {
                        parameters.Add($"height={height}");
                    }
                    
                    var version = media.UpdateDate != default ? media.UpdateDate.Ticks.ToString() : DateTime.UtcNow.Ticks.ToString();
                    
                    string finalUrl;
                    if (parameters.Any())
                    {
                        finalUrl = $"{url}?{string.Join("&", parameters)}&v={version}";
                    }
                    else if (!url.Contains("?"))
                    {
                        finalUrl = $"{url}?v={version}";
                    }
                    else
                    {
                        finalUrl = $"{url}&v={version}";
                    }

                    return finalUrl;
                }
                catch (Exception)
                {
                    return null;
                }
            }, TimeSpan.FromHours(1));
        }

        public async Task<string?> GetCachedMediaUrlAsync(IPublishedContent? media, string? cropAlias = null, int? width = null, int? height = null)
        {
            if (media == null)
            {
                return null;
            }
            
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
                    if (media == null)
                    {
                        return new HtmlString(string.Empty);
                    }
                    
                    var url = await GetCachedMediaUrlAsync(mediaKey, cropAlias);
                    if (string.IsNullOrEmpty(url))
                    {
                        return new HtmlString(string.Empty);
                    }

                    string? altText = media.GetValue<string>("altText") ?? media.Name;
                    string? title = media.GetValue<string>("title") ?? string.Empty;
                    
                    var html = GeneratePictureElement(url, altText, title, 
                        media.GetValue<int?>("width"), 
                        media.GetValue<int?>("height"), 
                        true);

                    return new HtmlString(html);
                }
                catch (Exception)
                {
                    return new HtmlString(string.Empty);
                }
            }, TimeSpan.FromMinutes(30));
        }

        public async Task<IHtmlContent> GetCachedImageHtmlAsync(IPublishedContent? media, string? cropAlias = null, Dictionary<string, string>? attributes = null)
        {
            if (media == null)
            {
                return new HtmlString(string.Empty);
            }
            
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
                
                return cssBuilder.ToString();
            }, TimeSpan.FromHours(1));

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
            _runtimeCache.ClearByKey($"media_url_{mediaKey}");
            _runtimeCache.ClearByKey($"media_html_{mediaKey}");
        }

        public void ClearAllCache()
        {
            _runtimeCache.Clear();
        }

        public string GeneratePictureElement(string? url, string? altText = "", string? title = "", int? width = null, int? height = null, bool lazyLoad = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            
            var webpUrl = ConvertToWebP(url);
            var srcset = width.HasValue ? GenerateSrcSet(url, null, width.Value) : string.Empty;
            
            var html = new StringBuilder();
            
            html.AppendLine("<picture>");
            html.AppendLine($"  <source srcset=\"{webpUrl}\" type=\"image/webp\">");
            
            if (!string.IsNullOrEmpty(srcset))
            {
                html.AppendLine($"  <source srcset=\"{srcset}\">");
            }
            
            html.Append($"  <img src=\"{url}\"");
            
            if (!string.IsNullOrEmpty(altText))
            {
                html.Append($" alt=\"{altText}\"");
            }
            
            if (!string.IsNullOrEmpty(title))
            {
                html.Append($" title=\"{title}\"");
            }
            
            if (lazyLoad)
            {
                html.Append(" loading=\"lazy\" decoding=\"async\"");
            }
            
            html.Append(" class=\"optimized-image\"");
            
            if (width.HasValue)
            {
                html.Append($" width=\"{width}\"");
            }
            
            if (height.HasValue)
            {
                html.Append($" height=\"{height}\"");
            }
            
            html.AppendLine(">");
            html.AppendLine("</picture>");
            
            return html.ToString();
        }

        public string ConvertToWebP(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            
            return url.Contains("?") ? $"{url}&format=webp" : $"{url}?format=webp";
        }

        public string GenerateSrcSet(string? url, string? cropAlias, int baseWidth)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            
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