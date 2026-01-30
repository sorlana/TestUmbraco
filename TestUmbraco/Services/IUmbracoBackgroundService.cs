// Services/IUmbracoBackgroundService.cs
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public interface IUmbracoBackgroundService
    {
        BackgroundResult ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg");
    }
}