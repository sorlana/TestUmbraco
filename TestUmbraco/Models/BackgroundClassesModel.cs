using System;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Models
{
    public class BackgroundClassesModel
    {
        public IPublishedElement? Settings { get; set; }
        public Guid? ComponentId { get; set; }
        public string? Prefix { get; set; }
        public string BaseClass { get; set; } = string.Empty;
        public string AdditionalClasses { get; set; } = string.Empty;
        public string Tag { get; set; } = "div";
        public string ContainerClass { get; set; } = string.Empty;
        public string ElementId { get; set; } = string.Empty;
        public Func<object, IHtmlContent>? Content { get; set; }
        public string RawContent { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
    }
}