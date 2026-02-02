using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;
using TestUmbraco.Services;
using TestUmbraco.Models;
using System.Text.Encodings.Web;
using System.IO;

namespace TestUmbraco.Helpers
{
    public class ImageHelper
    {
        private readonly IMediaCacheService _mediaCacheService;

        public ImageHelper(IMediaCacheService mediaCacheService)
        {
            _mediaCacheService = mediaCacheService;
        }

        public async Task<ImageModel?> GetImageModelAsync(IPublishedContent? content, string propertyAlias = "img")
        {
            if (content == null) return null;

            var mediaItem = content.Value<IPublishedContent>(propertyAlias);
            if (mediaItem == null) return null;

            return new ImageModel
            {
                Url = await _mediaCacheService.GetCachedMediaUrlAsync(mediaItem.Key),
                AltText = mediaItem.Value<string>("altText") ?? mediaItem.Name,
                Title = mediaItem.Value<string>("title"),
                Width = mediaItem.Value<int?>("width"),
                Height = mediaItem.Value<int?>("height"),
                FocalPoint = mediaItem.Value<string>("focalPoint")
            };
        }

        // ? Специальный метод для использования в представлениях (возвращает string)
        public string RenderImageForView(
            IPublishedContent? content, 
            string propertyAlias = "img", 
            string? cropAlias = null,
            Dictionary<string, string>? attributes = null,
            bool lazyLoad = true)
        {
            var mediaItem = content?.Value<IPublishedContent>(propertyAlias);
            if (mediaItem == null) return string.Empty;

            // Синхронное получение HTML
            var html = _mediaCacheService.GetCachedImageHtmlAsync(mediaItem, cropAlias, attributes)
                .GetAwaiter()
                .GetResult();
                
            // Преобразуем IHtmlContent в строку
            using var writer = new StringWriter();
            html.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        // ? Старый метод для обратной совместимости
        public IHtmlContent RenderImage(
            IPublishedContent? content, 
            string propertyAlias = "img", 
            string? cropAlias = null,
            Dictionary<string, string>? attributes = null,
            bool lazyLoad = true)
        {
            var htmlString = RenderImageForView(content, propertyAlias, cropAlias, attributes, lazyLoad);
            return new HtmlString(htmlString);
        }

        // ? Асинхронный метод для использования в контроллерах
        public async Task<IHtmlContent> RenderImageAsync(
            IPublishedContent? content, 
            string propertyAlias = "img", 
            string? cropAlias = null,
            Dictionary<string, string>? attributes = null,
            bool lazyLoad = true)
        {
            var mediaItem = content?.Value<IPublishedContent>(propertyAlias);
            if (mediaItem == null) return new HtmlString(string.Empty);

            return await _mediaCacheService.GetCachedImageHtmlAsync(mediaItem, cropAlias, attributes);
        }

        public async Task<string?> GetImageUrlAsync(
            IPublishedContent? content, 
            string propertyAlias = "img",
            string? cropAlias = null,
            int? width = null,
            int? height = null)
        {
            var mediaItem = content?.Value<IPublishedContent>(propertyAlias);
            if (mediaItem == null) return null;

            return await _mediaCacheService.GetCachedMediaUrlAsync(mediaItem, cropAlias, width, height);
        }
    }
}