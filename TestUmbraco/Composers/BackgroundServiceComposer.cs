// Composers/BackgroundServiceComposer.cs
using Microsoft.Extensions.DependencyInjection;
using TestUmbraco.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace TestUmbraco.Composers
{
    public class BackgroundServiceComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped<IUmbracoBackgroundService, UmbracoBackgroundService>();
            builder.Services.AddHttpContextAccessor();
        }
    }
}