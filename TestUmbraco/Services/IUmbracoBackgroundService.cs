// Services/IUmbracoBackgroundService.cs
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public interface IUmbracoBackgroundService
    {
        BackgroundResult ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg");
    }

    public class BackgroundResult
    {
        public string CssClass { get; set; } = string.Empty;
        public string ApiParam { get; set; } = string.Empty;
        public bool HasBackground { get; set; }
    }
}