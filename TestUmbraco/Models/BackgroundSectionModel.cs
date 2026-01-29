using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace TestUmbraco.Models
{
    public class BackgroundSectionModel
    {
        public IPublishedElement? Settings { get; set; } // Делаем nullable
        public Guid ComponentId { get; set; }
        public string Prefix { get; set; } = "bg"; // Значение по умолчанию
        public string SectionClass { get; set; } = string.Empty; // Инициализация
        public Func<object, IHtmlContent>? Content { get; set; } // Делаем nullable
        public string ContainerClass { get; set; } = string.Empty; // Инициализация
    }
}