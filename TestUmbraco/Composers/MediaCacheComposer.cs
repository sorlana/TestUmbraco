// MediaCacheComposer.cs
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace TestUmbraco.Composers
{
    public class MediaCacheComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // В Umbraco 17 контроллеры регистрируются автоматически
            // Не нужно регистрировать контроллеры вручную
        }
    }
}