using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public interface IMediaCacheService
    {
        // Основные методы получения URL
        Task<string?> GetCachedMediaUrlAsync(Guid mediaKey, string? cropAlias = null, int? width = null, int? height = null);
        Task<string?> GetCachedMediaUrlAsync(IPublishedContent? media, string? cropAlias = null, int? width = null, int? height = null);
        
        // Методы для HTML генерации
        Task<IHtmlContent> GetCachedImageHtmlAsync(Guid mediaKey, string? cropAlias = null, Dictionary<string, string>? attributes = null);
        Task<IHtmlContent> GetCachedImageHtmlAsync(IPublishedContent? media, string? cropAlias = null, Dictionary<string, string>? attributes = null);
        
        // Фоновые изображения
        Task<string> GetCachedBackgroundCssAsync(string backgrounds);
        Task<string> GenerateBackgroundCssAsync(Guid mediaGuid, string className, int minHeight = 400, string size = "cover", string position = "center");
        
        // Управление кешем
        void ClearCacheForMedia(Guid mediaKey);
        void ClearAllCache();
        
        // Оптимизация изображений
        string GeneratePictureElement(string? url, string? altText = "", string? title = "", int? width = null, int? height = null, bool lazyLoad = true);
        string ConvertToWebP(string? url);
        string GenerateSrcSet(string? url, string? cropAlias, int baseWidth);
    }
}