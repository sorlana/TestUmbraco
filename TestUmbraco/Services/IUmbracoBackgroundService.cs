using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public interface IUmbracoBackgroundService
    {
        Task<BackgroundResult> ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg");
    }
}