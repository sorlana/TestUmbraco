// Models/BackgroundSectionModel.cs
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Models
{
    public class BackgroundSectionModel
    {
        public IPublishedElement? Settings { get; set; }
        public Guid ComponentId { get; set; }
        public string Prefix { get; set; } = "bg";
        public string? BaseClass { get; set; }
        public string? AdditionalClasses { get; set; }
        public string? ContainerClass { get; set; }
        public Func<object, IHtmlContent>? Content { get; set; }
    }
}