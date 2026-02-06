using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using TestUmbraco.Services;

namespace TestUmbraco.Helpers
{
    public static class BackgroundHtmlExtensions
    {
        public static async Task<string> GetBackgroundClasses(
            this IHtmlHelper html,
            IPublishedElement settings,
            Guid componentId,
            string prefix = "bg")
        {
            var backgroundService = html.ViewContext.HttpContext.RequestServices
                .GetRequiredService<IUmbracoBackgroundService>();

            var result = await backgroundService.ProcessBackground(settings, componentId, prefix);
            return result?.CssClass ?? "";
        }

        public static string AddBackgroundClasses(
            this IHtmlHelper html,
            string baseClasses,
            string backgroundClasses)
        {
            var allClasses = new List<string>();

            if (!string.IsNullOrWhiteSpace(baseClasses))
                allClasses.Add(baseClasses.Trim());

            if (!string.IsNullOrWhiteSpace(backgroundClasses))
                allClasses.Add(backgroundClasses.Trim());

            return string.Join(" ", allClasses);
        }
    }
}