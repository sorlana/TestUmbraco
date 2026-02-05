using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;
using TestUmbraco.Services;

namespace TestUmbraco.Helpers
{
    public class ImageHelper
    {
        private readonly IMediaCacheService _mediaCacheService;

        public ImageHelper(IMediaCacheService mediaCacheService)
        {
            _mediaCacheService = mediaCacheService;
        }

        public async Task<IHtmlContent> RenderMediaAsync(
            IPublishedContent media,
            string cropAlias = null,
            Dictionary<string, string> attributes = null,
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
    }
}