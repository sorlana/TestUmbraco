using Microsoft.Extensions.DependencyInjection;
using TestUmbraco.Helpers;
using TestUmbraco.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace TestUmbraco.Composers
{
    public class MediaCacheComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IMediaCacheService, MediaCacheService>();
            builder.Services.AddSingleton<IStaticCssGeneratorService, StaticCssGeneratorService>();
            builder.Services.AddScoped<IUmbracoBackgroundService, UmbracoBackgroundService>();
            builder.Services.AddScoped<ImageHelper>();
            
            builder.AddNotificationHandler<MediaSavedNotification, MediaCacheNotificationHandler>();
            builder.AddNotificationHandler<MediaDeletedNotification, MediaCacheNotificationHandler>();
            builder.AddNotificationHandler<MediaMovedNotification, MediaCacheNotificationHandler>();
        }
    }
    
    public class MediaCacheNotificationHandler : 
        INotificationHandler<MediaSavedNotification>,
        INotificationHandler<MediaDeletedNotification>,
        INotificationHandler<MediaMovedNotification>
    {
        private readonly IMediaCacheService _mediaCacheService;
        private readonly IStaticCssGeneratorService _staticCssGenerator;
        private readonly ILoggingService _loggingService;

        public MediaCacheNotificationHandler(
            IMediaCacheService mediaCacheService,
            IStaticCssGeneratorService staticCssGenerator,
            ILoggingService loggingService)
        {
            _mediaCacheService = mediaCacheService;
            _staticCssGenerator = staticCssGenerator;
            _loggingService = loggingService;
        }

        public async void Handle(MediaSavedNotification notification)
        {
            foreach (var media in notification.SavedEntities)
            {
                _mediaCacheService.ClearCacheForMedia(media.Key);
                
                // Обновляем статический CSS файл
                await _staticCssGenerator.UpdateCssForMediaAsync(media.Key);
                
                _loggingService.LogInformation<MediaCacheNotificationHandler>($"Auto-cleared cache and updated CSS for saved media: {media.Key}");
            }
        }

        public async void Handle(MediaDeletedNotification notification)
        {
            foreach (var media in notification.DeletedEntities)
            {
                _mediaCacheService.ClearCacheForMedia(media.Key);
                
                // Удаляем из статического CSS файла
                await _staticCssGenerator.RemoveCssForMediaAsync(media.Key);
                
                _loggingService.LogInformation<MediaCacheNotificationHandler>($"Auto-cleared cache and removed CSS for deleted media: {media.Key}");
            }
        }

        public async void Handle(MediaMovedNotification notification)
        {
            foreach (var moveEvent in notification.MoveInfoCollection)
            {
                _mediaCacheService.ClearCacheForMedia(moveEvent.Entity.Key);
                
                // Обновляем статический CSS файл
                await _staticCssGenerator.UpdateCssForMediaAsync(moveEvent.Entity.Key);
                
                _loggingService.LogInformation<MediaCacheNotificationHandler>($"Auto-cleared cache and updated CSS for moved media: {moveEvent.Entity.Key}");
            }
        }
    }
}