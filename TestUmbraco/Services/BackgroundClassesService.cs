using System;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using TestUmbraco.Services;

namespace TestUmbraco.Services
{
    public class BackgroundClassesService
    {
        private readonly IUmbracoBackgroundService _backgroundService;

        public BackgroundClassesService(IUmbracoBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        public async Task<string> GetBackgroundClasses(
            IPublishedElement settings,
            Guid componentId,
            string prefix = "bg")
        {
            var result = await _backgroundService.ProcessBackground(settings, componentId, prefix);
            return result?.CssClass ?? "";
        }

        public async Task<BackgroundResult> GetBackgroundData(
            IPublishedElement settings,
            Guid componentId,
            string prefix = "bg")
        {
            return await _backgroundService.ProcessBackground(settings, componentId, prefix);
        }
    }
}