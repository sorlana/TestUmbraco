// Services/IUmbracoBackgroundService.cs
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Services
{
    public interface IUmbracoBackgroundService
    {
        BackgroundResult ProcessBackground(IPublishedElement? settings, Guid componentId, string prefix = "bg");
    }

    public enum BackgroundType
    {
        None,
        Image,
        Color,
        Gradient,
        Video
    }

    public class BackgroundResult
    {
        public BackgroundType Type { get; set; } = BackgroundType.None;
        public string CssClass { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string CssContent { get; set; } = string.Empty;
        public bool HasBackground { get; set; }
    }
}